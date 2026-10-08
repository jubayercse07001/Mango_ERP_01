using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using ERP.DAL;
using ERP.DAL.HR.Organization;

using ERP.Models.User;
using ERP.Models.HR.Organization.HrMainDivisionDesignationMap;
using System.Data.SqlClient;

namespace ERP.Controllers.HR.Organization
{
    public class HrMainDivisionDesignationMapController : Controller
    {
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
                return Redirect(
                    Url.Action("Index", "HrMainDivisionDesignationMap"));

            ViewBag.MainDivisionList =
                HrMainDivisionDAL.GetAllHrMainDivisions();

            ViewBag.DesignationList =
                HrDesignationDAL.GetAllHrDesignations();

            ViewBag.User = user;

            return View();
        }

        [HttpPost]
        public ActionResult Add(
            HrMainDivisionDesignationMapInfo map)
        {
            HrMainDivisionDesignationMapResponse response =
                new HrMainDivisionDesignationMapResponse();

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
            
            if (string.IsNullOrWhiteSpace(map.MainDivisionId))
                response.ResponseMessages.Add(
                    "Main Division is required");

            if (string.IsNullOrWhiteSpace(map.DesignationId))
                response.ResponseMessages.Add(
                    "Designation is required");
            List<string> designationList = new List<string>();

             designationList = (map.DesignationId ?? "")
                                .Split(',')
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .ToList();

            if (designationList.Count == 0)
            {
                response.ResponseMessages.Add(
                    "At least one designation is required");
            }
            

            foreach (var designationId in designationList)
            {
                if (HrMainDivisionDesignationMapDAL
                    .GetDuplicateMap(
                        map.MainDivisionId,
                        designationId) != null)
                {
                    response.ResponseMessages.Add(
                        "Duplicate designation found");
                    break;
                }
            }

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;

                response.ResponseMessage =
                    "<ul><li>" +
                    response.ResponseMessages.Aggregate(
                        (i, j) => i + "</li><li>" + j)
                    + "</li></ul>";

                return Json(response);
            }

            try
            {
                foreach (var designationId in designationList)
                {
                    HrMainDivisionDesignationMapInfo obj =
                        new HrMainDivisionDesignationMapInfo();

                    obj.RowId = Guid.NewGuid().ToString();
                    obj.MainDivisionId = map.MainDivisionId;
                    obj.DesignationId = designationId;

                    System.Diagnostics.Debug.WriteLine(obj.DesignationId);

                    HrMainDivisionDesignationMapDAL.AddMap(obj, lgUser);
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseMessage = ex.Message;

                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage =
                "Mapping added successfully";

            response.ResponseMessages = new List<string>();


            return Json(response);
        }

        public ActionResult GetHrMainDivisionDesignationMapItemList()
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

            HrMainDivisionDesignationMapResponse resp =
                HrMainDivisionDesignationMapDAL.GetFilteredHrMainDivisionDesignationMaps(
                    Request);

            return Json(new
            {
                data = resp.HrMainDivisionDesignationMaps,
                draw = Request["draw"],
                recordsTotal = resp.TotalRecordCount,
                recordsFiltered = resp.FilteredRecordCount
            },
            JsonRequestBehavior.AllowGet);
        }

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

            if (user.PermissionGroup[2] != '1')
                return Redirect(Url.Action("Index", "HrMainDivisionDesignationMap"));

            // 1. Get the clicked row map entry context to find out which Division we are editing
            HrMainDivisionDesignationMapInfo primaryMap = HrMainDivisionDesignationMapDAL.GetMapByID(id);
            if (primaryMap == null)
            {
                return Redirect(Url.Action("Index", "HrMainDivisionDesignationMap"));
            }

            // 2. Fetch all saved assignments currently matching this MainDivisionId 
            List<HrMainDivisionDesignationMapInfo> savedMaps = HrMainDivisionDesignationMapDAL.GetDesignationByMainDivision(primaryMap.MainDivisionId);

            // 3. Map the assigned Designation IDs to a string list container inside the model
            primaryMap.DesignationIds = savedMaps.Select(x => x.DesignationId).ToList();

            ViewBag.MainDivisionList = HrMainDivisionDAL.GetAllHrMainDivisions();
            ViewBag.DesignationList = HrDesignationDAL.GetAllHrDesignations();
            ViewBag.User = user;

            return View(primaryMap);
        }

        [HttpPost]
        public ActionResult Edit(HrMainDivisionDesignationMapInfo map)
        {
            HrMainDivisionDesignationMapResponse response = new HrMainDivisionDesignationMapResponse();
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

            // Basic Validation Rules
            if (string.IsNullOrWhiteSpace(map.MainDivisionId))
                response.ResponseMessages.Add("Main Division is required");

            if (string.IsNullOrWhiteSpace(map.DesignationId))
                response.ResponseMessages.Add("Designation is required");

            // Process designations from comma-separated input string
            List<string> designationList = (map.DesignationId ?? "")
                                            .Split(',')
                                            .Where(x => !string.IsNullOrWhiteSpace(x))
                                            .Distinct() // Drop UI duplicates automatically
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
                    // 1. Clear existing database maps assigned under this specific division context
                    // This safely mimics an atomic update routine
                    repo.Database.ExecuteSqlCommand(@"
                UPDATE HrMainDivisionDesignationMap 
                SET DomainStatus = 0, UpdateUser = @UpdateUser, UpdateDate = GETDATE() 
                WHERE MainDivisionId = @MainDivisionId AND DomainStatus = 1",
                        new SqlParameter("@MainDivisionId", map.MainDivisionId),
                        new SqlParameter("@UpdateUser", lgUser));
                }

                // 2. Loop through and add the new list of designations using your AddMap routine
                foreach (var designationId in designationList)
                {
                    HrMainDivisionDesignationMapInfo obj = new HrMainDivisionDesignationMapInfo
                    {
                        RowId = Guid.NewGuid().ToString(),
                        MainDivisionId = map.MainDivisionId,
                        DesignationId = designationId
                    };

                    HrMainDivisionDesignationMapDAL.AddMap(obj, lgUser);
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

            if (user.PermissionGroup[3] != '1') // Delete permission authorization check
                return Redirect(Url.Action("Index", "HrMainDivisionDesignationMap"));

            // 1. Fetch the primary mapping record using the incoming row tracking code
            HrMainDivisionDesignationMapInfo primaryMap = HrMainDivisionDesignationMapDAL.GetMapByID(id);

            if (primaryMap == null)
            {
                return Redirect(Url.Action("Index", "HrMainDivisionDesignationMap"));
            }

            // 2. Query all sibling designations grouped under the exact same Main Division
            List<HrMainDivisionDesignationMapInfo> mappedGroupList =
                HrMainDivisionDesignationMapDAL.GetDesignationByMainDivision(primaryMap.MainDivisionId);

            // 3. Stash the list inside a ViewBag item container to display them in a table row loop
            ViewBag.GroupedDesignations = mappedGroupList;
            ViewBag.User = user;

            return View(primaryMap);
        }

        [HttpPost]
        public ActionResult Delete(HrMainDivisionDesignationMapInfo map)
        {
            HrMainDivisionDesignationMapResponse response = new HrMainDivisionDesignationMapResponse();

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
                    // Execute a mass-update statement targeting all valid mapping paths inside this Division
                    repo.Database.ExecuteSqlCommand(@"
                UPDATE HrMainDivisionDesignationMap
                SET
                     DomainStatus = 0
                    ,UpdateUser = @UpdateUser
                    ,UpdateDate = GETDATE()
                WHERE MainDivisionId = @MainDivisionId
                AND DomainStatus = 1
            ",
                    new SqlParameter("MainDivisionId", map.MainDivisionId),
                    new SqlParameter("UpdateUser", lgUser));
                }

                response.ResponseStatus = 1;
                response.ResponseMessage = "All designations under this division have been successfully unmapped.";
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