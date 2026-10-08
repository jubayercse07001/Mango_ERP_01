using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using ERP.DAL;
using ERP.DAL.HR.Organization;

using ERP.Models.User;
using ERP.Models.HR.Organization.HrDivisionDesignationMap;
using System.Data.SqlClient;
using ERP.DAL.HR.Common;

namespace ERP.Controllers.HR.Organization
{
    public class HrDivisionDesignationMapController : Controller
    {
        // GET: HrDivisionDesignationMap
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

            if (user.PermissionGroup[0] != '1')
                return Redirect(Url.Action("Index", "Home"));

            ViewBag.User = user;
            return View();
        }

        // GET: HrDivisionDesignationMap/Add
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

            if (user.PermissionGroup[1] != '1')
                return Redirect(Url.Action("Index", "HrDivisionDesignationMap"));

            // Load master records for dropdown lists
            ViewBag.DivisionList = HrDivisionDAL.GetAllHrDivisions(); // Assumed matching your master data setup
            ViewBag.DesignationList = HrDesignationDAL.GetAllHrDesignations();
            ViewBag.User = user;

            return View();
        }

        // POST: HrDivisionDesignationMap/Add
        [HttpPost]
        public ActionResult Add(HrDivisionDesignationMapInfo map)
        {
            HrDivisionDesignationMapResponse response = new HrDivisionDesignationMapResponse();
            string lgUser = Session["LoggedInUser"]?.ToString() ?? "";

            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                return Json(response);
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null || user.PermissionGroup[1] != '1')
            {
                response.ResponseStatus = -2;
                return Json(response);
            }

            // Validations
            if (string.IsNullOrWhiteSpace(map.DivisionId))
                response.ResponseMessages.Add("Division is required");

            if (string.IsNullOrWhiteSpace(map.DesignationId))
                response.ResponseMessages.Add("Designation is required");

            // Split selected designations out of CSV string format
            List<string> designationList = (map.DesignationId ?? "")
                                            .Split(',')
                                            .Where(x => !string.IsNullOrWhiteSpace(x))
                                            .ToList();

            if (designationList.Count == 0)
            {
                response.ResponseMessages.Add("At least one designation is required");
            }

            // Check for potential duplicate mappings across the list
            foreach (var designationId in designationList)
            {
                if (HrDivisionDesignationMapDAL.GetDuplicateMap(map.DivisionId, designationId) != null)
                {
                    response.ResponseMessages.Add("Duplicate designation mapping found in database context");
                    break;
                }
            }

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseMessage = "<ul><li>" + response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(response);
            }

            try
            {
                foreach (var designationId in designationList)
                {
                    HrDivisionDesignationMapInfo obj = new HrDivisionDesignationMapInfo
                    {
                        RowId = Guid.NewGuid().ToString(),
                        DivisionId = map.DivisionId,
                        DesignationId = designationId
                    };

                    HrDivisionDesignationMapDAL.AddMap(obj, lgUser);
                }

                response.ResponseStatus = 1;
                response.ResponseMessage = "Mapping added successfully";
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseMessage = ex.Message;
            }

            return Json(response);
        }

        // GET: AJAX Paging Grid Source API Link
        public ActionResult GetHrDivisionDesignationMapItemList()
        {
            string lgUser = Session["LoggedInUser"]?.ToString() ?? "";

            if (string.IsNullOrEmpty(lgUser))
            {
                return Json(new { data = new List<object>() }, JsonRequestBehavior.AllowGet);
            }

            HrDivisionDesignationMapResponse resp = HrDivisionDesignationMapDAL.GetFilteredHrDivisionDesignationMaps(Request);

            return Json(new
            {
                data = resp.HrDivisionDesignationMaps,
                draw = Request["draw"],
                recordsTotal = resp.TotalRecordCount,
                recordsFiltered = resp.FilteredRecordCount
            }, JsonRequestBehavior.AllowGet);
        }

        // GET: HrDivisionDesignationMap/Edit/id
        [HttpGet]
        public ActionResult Edit(string id)
        {
            string lgUser = Session["LoggedInUser"]?.ToString() ?? "";
            if (string.IsNullOrEmpty(lgUser))
                return Redirect(Url.Action("Logout", "Login"));

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null || user.PermissionGroup[2] != '1')
                return Redirect(Url.Action("Index", "HrDivisionDesignationMap"));

            // 1. Fetch the primary reference record clicked on from the listing index grid
            HrDivisionDesignationMapInfo primaryMap = HrDivisionDesignationMapDAL.GetMapByID(id);
            if (primaryMap == null)
            {
                return Redirect(Url.Action("Index", "HrDivisionDesignationMap"));
            }

            // 2. Fetch all related sister mappings mapped underneath this exact DivisionId context
            List<HrDivisionDesignationMapInfo> savedMaps = HrDivisionDesignationMapDAL.GetDesignationByDivision(primaryMap.DivisionId);

            // 3. Populate entity list tracking collection properties
            primaryMap.DesignationIds = savedMaps.Select(x => x.DesignationId).ToList();

            ViewBag.DivisionList = HrDivisionDAL.GetAllHrDivisions();
            ViewBag.DesignationList = HrDesignationDAL.GetAllHrDesignations();
            ViewBag.User = user;

            return View(primaryMap);
        }

        // POST: HrDivisionDesignationMap/Edit
        [HttpPost]
        public ActionResult Edit(HrDivisionDesignationMapInfo map)
        {
            HrDivisionDesignationMapResponse response = new HrDivisionDesignationMapResponse();
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

            if (string.IsNullOrWhiteSpace(map.DivisionId))
                response.ResponseMessages.Add("Division is required");

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
                    // Clean down any prior existing records matching this target context node
                    repo.Database.ExecuteSqlCommand(@"
                        UPDATE HrDivisionDesignationMap 
                        SET DomainStatus = 0, UpdateUser = @UpdateUser, UpdateDate = GETDATE() 
                        WHERE DivisionId = @DivisionId AND DomainStatus = 1",
                        new SqlParameter("@DivisionId", map.DivisionId),
                        new SqlParameter("@UpdateUser", lgUser));
                }

                // Batch append modern updated list definitions inside table rows
                foreach (var designationId in designationList)
                {
                    HrDivisionDesignationMapInfo obj = new HrDivisionDesignationMapInfo
                    {
                        RowId = Guid.NewGuid().ToString(),
                        DivisionId = map.DivisionId,
                        DesignationId = designationId
                    };

                    HrDivisionDesignationMapDAL.AddMap(obj, lgUser);
                }

                response.ResponseStatus = 1;
                response.ResponseMessage = "Mapping details updated successfully";
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseMessage = ex.Message;
            }

            return Json(response);
        }

        // GET: HrDivisionDesignationMap/Delete/id
        [HttpGet]
        public ActionResult Delete(string id)
        {
            string lgUser = Session["LoggedInUser"]?.ToString() ?? "";
            if (string.IsNullOrEmpty(lgUser))
                return Redirect(Url.Action("Logout", "Login"));

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null || user.PermissionGroup[3] != '1')
                return Redirect(Url.Action("Index", "HrDivisionDesignationMap"));

            HrDivisionDesignationMapInfo primaryMap = HrDivisionDesignationMapDAL.GetMapByID(id);
            if (primaryMap == null)
            {
                return Redirect(Url.Action("Index", "HrDivisionDesignationMap"));
            }

            // Gather all linked groupings together to expose them inside confirmation tables
            List<HrDivisionDesignationMapInfo> mappedGroupList = HrDivisionDesignationMapDAL.GetDesignationByDivision(primaryMap.DivisionId);

            ViewBag.GroupedDesignations = mappedGroupList;
            ViewBag.User = user;

            return View(primaryMap);
        }

        // POST: HrDivisionDesignationMap/Delete
        [HttpPost]
        public ActionResult Delete(HrDivisionDesignationMapInfo map)
        {
            HrDivisionDesignationMapResponse response = new HrDivisionDesignationMapResponse();
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
                    // Mass soft-delete update statement execution for this unique mapping scope
                    repo.Database.ExecuteSqlCommand(@"
                        UPDATE HrDivisionDesignationMap
                        SET
                             DomainStatus = 0
                            ,UpdateUser = @UpdateUser
                            ,UpdateDate = GETDATE()
                        WHERE DivisionId = @DivisionId
                        AND DomainStatus = 1",
                    new SqlParameter("DivisionId", map.DivisionId),
                    new SqlParameter("UpdateUser", lgUser));
                }

                response.ResponseStatus = 1;
                response.ResponseMessage = "All configurations under this structural sector dropped successfully.";
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