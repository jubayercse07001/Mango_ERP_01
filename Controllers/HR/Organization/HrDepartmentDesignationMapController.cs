using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;

using ERP.DAL;
using ERP.DAL.HR.Organization;
using ERP.Models.User;
using ERP.Models.HR.Organization.HrDepartmentDesignationMap;
using ERP.DAL.HR.Common;

namespace ERP.Controllers.HR.Organization
{
    public class HrDepartmentDesignationMapController : Controller
    {
        // ==========================================
        // 1. INDEX GRID VIEW
        // ==========================================
        public ActionResult Index()
        {
            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Redirect(Url.Action("Logout", "Login"));

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Redirect(Url.Action("Logout", "Login"));

            // Permission check index [0] matches Index/Read authorizations
            if (user.PermissionGroup[0] != '1')
                return Redirect(Url.Action("Index", "Home"));

            ViewBag.User = user;

            return View();
        }

        // ==========================================
        // 2. CREATE / MAP DESIGNATIONS TO DEPARTMENT
        // ==========================================
        [HttpGet]
        public ActionResult Add()
        {
            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Redirect(Url.Action("Logout", "Login"));

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Redirect(Url.Action("Logout", "Login"));

            // Permission check index [1] matches Create/Add authorizations
            if (user.PermissionGroup[1] != '1')
                return Redirect(Url.Action("Index", "HrDepartmentDesignationMap"));

            // Populate view-level dropdown matrices
            ViewBag.DepartmentList = HrDepartmentDAL.GetAllHrDepartments();
            ViewBag.DesignationList = HrDesignationDAL.GetAllHrDesignations();

            ViewBag.User = user;

            return View();
        }

        [HttpPost]
        public ActionResult Add(HrDepartmentDesignationMapInfo map)
        {
            HrDepartmentDesignationMapResponse response = new HrDepartmentDesignationMapResponse();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                return Json(response);
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
            {
                response.ResponseStatus = -1;
                return Json(response);
            }

            if (user.PermissionGroup[1] != '1')
            {
                response.ResponseStatus = -2;
                return Json(response);
            }

            // Domain validation checks
            if (string.IsNullOrWhiteSpace(map.DepartmentId))
                response.ResponseMessages.Add("Department is required");

            if (string.IsNullOrWhiteSpace(map.DesignationId))
                response.ResponseMessages.Add("Designation is required");

            List<string> designationList = (map.DesignationId ?? "")
                                                .Split(',')
                                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                                .ToList();

            if (designationList.Count == 0)
            {
                response.ResponseMessages.Add("At least one designation is required");
            }

            // Uniqueness validation verification loop
            foreach (var designationId in designationList)
            {
                if (HrDepartmentDesignationMapDAL.GetDuplicateMap(map.DepartmentId, designationId) != null)
                {
                    response.ResponseMessages.Add("Duplicate designation found");
                    break;
                }
            }

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseMessage = "<ul><li>" +
                                            response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j)
                                            + "</li></ul>";

                return Json(response);
            }

            try
            {
                // Iterate list and commit mapping configuration matrices
                foreach (var designationId in designationList)
                {
                    HrDepartmentDesignationMapInfo obj = new HrDepartmentDesignationMapInfo();

                    obj.RowId = Guid.NewGuid().ToString();
                    obj.DepartmentId = map.DepartmentId;
                    obj.DesignationId = designationId;

                    System.Diagnostics.Debug.WriteLine(obj.DesignationId);

                    HrDepartmentDesignationMapDAL.AddMap(obj, lgUser);
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseMessage = ex.Message;

                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "Mapping added successfully";
            response.ResponseMessages = new List<string>();

            return Json(response);
        }

        // ==========================================
        // 3. SERVER-SIDE DATA GRID SOURCE (DATATABLES)
        // ==========================================
        public ActionResult GetHrDepartmentDesignationMapItemList()
        {
            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
            {
                return Json(new
                {
                    data = new List<object>()
                }, JsonRequestBehavior.AllowGet);
            }

            HrDepartmentDesignationMapResponse resp = HrDepartmentDesignationMapDAL.GetFilteredHrDepartmentDesignationMaps(Request);

            return Json(new
            {
                data = resp.HrDepartmentDesignationMaps,
                draw = Request["draw"],
                recordsTotal = resp.TotalRecordCount,
                recordsFiltered = resp.FilteredRecordCount
            }, JsonRequestBehavior.AllowGet);
        }

        // ==========================================
        // 4. EDIT / UPDATE ARRANGEMENTS
        // ==========================================
        [HttpGet]
        public ActionResult Edit(string id)
        {
            string lgUser = "";
            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Redirect(Url.Action("Logout", "Login"));

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null)
                return Redirect(Url.Action("Logout", "Login"));

            // Permission check index [2] matches Write/Update actions
            if (user.PermissionGroup[2] != '1')
                return Redirect(Url.Action("Index", "HrDepartmentDesignationMap"));

            // 1. Fetch targeted tracking point mapping identity
            HrDepartmentDesignationMapInfo primaryMap = HrDepartmentDesignationMapDAL.GetMapByID(id);
            if (primaryMap == null)
            {
                return Redirect(Url.Action("Index", "HrDepartmentDesignationMap"));
            }

            // 2. Look up all mapped allocations active within this target Department footprint
            List<HrDepartmentDesignationMapInfo> savedMaps = HrDepartmentDesignationMapDAL.GetDesignationByDepartment(primaryMap.DepartmentId);

            // 3. Sync target IDs array back into view models
            primaryMap.DesignationIds = savedMaps.Select(x => x.DesignationId).ToList();

            ViewBag.DepartmentList = HrDepartmentDAL.GetAllHrDepartments();
            ViewBag.DesignationList = HrDesignationDAL.GetAllHrDesignations();
            ViewBag.User = user;

            return View(primaryMap);
        }

        [HttpPost]
        public ActionResult Edit(HrDepartmentDesignationMapInfo map)
        {
            HrDepartmentDesignationMapResponse response = new HrDepartmentDesignationMapResponse();
            string lgUser = Session["LoggedInUser"]?.ToString() ?? "";

            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                return Json(response);
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null || user.PermissionGroup[2] != '1')
            {
                response.ResponseStatus = -2;
                return Json(response);
            }

            if (string.IsNullOrWhiteSpace(map.DepartmentId))
                response.ResponseMessages.Add("Department is required");

            if (string.IsNullOrWhiteSpace(map.DesignationId))
                response.ResponseMessages.Add("Designation is required");

            List<string> designationList = (map.DesignationId ?? "")
                                                .Split(',')
                                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                                .Distinct()
                                                .ToList();

            if (designationList.Count == 0)
                response.ResponseMessages.Add("At least one designation selection is required");

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseMessage = "<ul><li>" + response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(response);
            }

            try
            {
                using (var repo = new GMContext())
                {
                    // Soft-delete transaction drop layer: clearing preexisting department assignments 
                    repo.Database.ExecuteSqlCommand(@"
                        UPDATE HrDepartmentDesignationMap 
                        SET DomainStatus = 0, UpdateUser = @UpdateUser, UpdateDate = GETDATE() 
                        WHERE DepartmentId = @DepartmentId AND DomainStatus = 1",
                        new SqlParameter("@DepartmentId", map.DepartmentId),
                        new SqlParameter("@UpdateUser", lgUser));
                }

                // Append the updated matrix mappings cleanly
                foreach (var designationId in designationList)
                {
                    HrDepartmentDesignationMapInfo obj = new HrDepartmentDesignationMapInfo
                    {
                        RowId = Guid.NewGuid().ToString(),
                        DepartmentId = map.DepartmentId,
                        DesignationId = designationId
                    };

                    HrDepartmentDesignationMapDAL.AddMap(obj, lgUser);
                }

                response.ResponseStatus = 1;
                response.ResponseMessage = "Mapping arrangement details updated successfully";
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseMessage = ex.Message;
            }

            return Json(response);
        }

        // ==========================================
        // 5. UNMAP / REMOVE ENTRIES
        // ==========================================
        [HttpGet]
        public ActionResult Delete(string id)
        {
            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Redirect(Url.Action("Logout", "Login"));

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Redirect(Url.Action("Logout", "Login"));

            // Permission check index [3] matches Delete transaction boundaries
            if (user.PermissionGroup[3] != '1')
                return Redirect(Url.Action("Index", "HrDepartmentDesignationMap"));

            HrDepartmentDesignationMapInfo primaryMap = HrDepartmentDesignationMapDAL.GetMapByID(id);

            if (primaryMap == null)
            {
                return Redirect(Url.Action("Index", "HrDepartmentDesignationMap"));
            }

            // Retrieve related mapping allocations for display warning context summaries
            List<HrDepartmentDesignationMapInfo> mappedGroupList = HrDepartmentDesignationMapDAL.GetDesignationByDepartment(primaryMap.DepartmentId);

            ViewBag.GroupedDesignations = mappedGroupList;
            ViewBag.User = user;

            return View(primaryMap);
        }

        [HttpPost]
        public ActionResult Delete(HrDepartmentDesignationMapInfo map)
        {
            HrDepartmentDesignationMapResponse response = new HrDepartmentDesignationMapResponse();

            string lgUser = Session["LoggedInUser"]?.ToString() ?? "";

            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                return Json(response);
            }

            try
            {
                using (var repo = new GMContext())
                {
                    // Wipe active assignments tied explicitly to this targeted department id field token
                    repo.Database.ExecuteSqlCommand(@"
                        UPDATE HrDepartmentDesignationMap
                        SET
                             DomainStatus = 0
                            ,UpdateUser = @UpdateUser
                            ,UpdateDate = GETDATE()
                        WHERE DepartmentId = @DepartmentId
                        AND DomainStatus = 1",
                        new SqlParameter("DepartmentId", map.DepartmentId),
                        new SqlParameter("UpdateUser", lgUser));
                }

                response.ResponseStatus = 1;
                response.ResponseMessage = "All designations under this department have been successfully unmapped.";
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseMessage = ex.Message;
            }

            return Json(response);
        }
    }
}