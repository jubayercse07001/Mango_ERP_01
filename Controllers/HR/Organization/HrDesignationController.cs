using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using ERP.DAL;
using ERP.DAL.HR.Common;
using ERP.DAL.HR.Organization;

using ERP.Models.User;
using ERP.Models.HR.Organization.HrDesignation;

namespace ERP.Controllers.HR.Organization
{
    public class HrDesignationController : Controller
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
        public JsonResult GetNextRank(string companyId, string branchId, string departmentId)
        {
            int nextRank = HrApprovedPositionDAL.GetNextPositionRank(companyId, branchId, departmentId);
            return Json(nextRank, JsonRequestBehavior.AllowGet);
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
                return Redirect(Url.Action("Index", "HrDesignation"));

            ViewBag.User = user;
            var designations = HrDesignationDAL.GetAllHrDesignations();
            ViewBag.Designation = designations ?? new List<HrDesignationInfo>();
            ViewBag.NextRank = designations != null && designations.Any()
                               ? designations.Max(x => x.Rank) + 1
                               : 1;
            ViewBag.DesignationGroupList = HrDesignationGroupDAL.GetAllHrDesignationGroups();
            return View();
        }

        [HttpPost]
        public ActionResult Add(HrDesignationInfo HrDesignation)
        {
            HrDesignationResponse response =
                new HrDesignationResponse();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                response.ResponseCode = "Not Logged In";
                return Json(response);
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
            {
                response.ResponseStatus = -1;
                response.ResponseCode = "Invalid User";
                return Json(response);
            }

            if (user.PermissionGroup[1] != '1')
            {
                response.ResponseStatus = -2;
                response.ResponseCode = "Not Authorized";
                return Json(response);
            }

            HrDesignation.RowId = Guid.NewGuid().ToString();
            if (string.IsNullOrWhiteSpace(HrDesignation.DesignationName))
                response.ResponseMessages.Add("Designation Name is required");

            else if (HrDesignationDAL.GetHrDesignationByName(HrDesignation.DesignationName, HrDesignation.RowId) != null)
                response.ResponseMessages.Add("Designation Name already exists");
            else if (HrDesignation.Rank <= 0)
            {
                response.ResponseMessages.Add("Rank is required");
            }
            
            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";

                response.ResponseMessage =
                    "<ul><li>" +
                    response.ResponseMessages.Aggregate(
                        (i, j) => i + "</li><li>" + j)
                    + "</li></ul>";

                return Json(response);
            }

            try
            {
                HrDesignation.RowId =
                    Guid.NewGuid().ToString();

                HrDesignationDAL.AddHrDesignation(
                    HrDesignation,
                    lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;

                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseCode = "Success";
            response.ResponseMessage =
                "Designation added successfully";

            return Json(response);
        }

        public ActionResult GetHrDesignationItemList()
        {
            List<HrDesignationInfo> list =
                new List<HrDesignationInfo>();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Json(list, JsonRequestBehavior.AllowGet);

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Json(list, JsonRequestBehavior.AllowGet);

            HrDesignationResponse resp = HrDesignationDAL.GetFilteredHrDesignations(Request);

            return Json(new
            {
                data = resp.HrDesignations,
                draw = Request["draw"],
                recordsTotal = resp.TotalRecordCount,
                recordsFiltered = resp.FilteredRecordCount
            },
            JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Edit()
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
                return Redirect(
                    Url.Action("Index", "HrDesignation"));

            HrDesignationInfo designation = HrDesignationDAL.GetDesignationByID(
                    RouteData.Values["id"].ToString());

            if (designation == null)
                return Redirect(
                    Url.Action("Index", "HrDesignation"));

            ViewBag.User = user;
            ViewBag.HrDesignation = designation;
            ViewBag.DesignationGroupList = HrDesignationGroupDAL.GetAllHrDesignationGroups();
            return View();
        }

        [HttpPost]
        public ActionResult Edit(HrDesignationInfo HrDesignation)
        {
            HrDesignationResponse response =
                new HrDesignationResponse();

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

            if (user.PermissionGroup[2] != '1')
            {
                response.ResponseStatus = -2;
                return Json(response);
            }

            if (string.IsNullOrWhiteSpace(
                HrDesignation.DesignationName))
            {
                response.ResponseMessages.Add(
                    "Designation Name is required");
            }
            else if (HrDesignationDAL.GetHrDesignationByName(
                HrDesignation.DesignationName,
                HrDesignation.RowId) != null)
            {
                response.ResponseMessages.Add(
                    "Designation Name already exists");
            }

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";

                response.ResponseMessage =
                    "<ul><li>" +
                    response.ResponseMessages.Aggregate(
                        (i, j) => i + "</li><li>" + j)
                    + "</li></ul>";

                return Json(response);
            }

            try
            {
                HrDesignationDAL.UpdateHrDesignation(
                    HrDesignation,
                    lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;

                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage =
                "Designation updated successfully";

            return Json(response);
        }

        [HttpGet]
        public ActionResult Delete()
        {
            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Redirect(Url.Action("Logout", "Login"));

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Redirect(Url.Action("Logout", "Login"));

            if (user.PermissionGroup[3] != '1')
                return Redirect(
                    Url.Action("Index", "HrDesignation"));

            HrDesignationInfo designation =
                HrDesignationDAL.GetDesignationByID(
                    RouteData.Values["id"].ToString());

            if (designation == null)
                return Redirect(
                    Url.Action("Index", "HrDesignation"));

            ViewBag.User = user;
            ViewBag.HrDesignation = designation;

            return View();
        }

        [HttpPost]
        public ActionResult Delete(
            HrDesignationInfo HrDesignation)
        {
            HrDesignationResponse response =
                new HrDesignationResponse();

            string lgUser =
                Session["LoggedInUser"]?.ToString();

            try
            {
                HrDesignationDAL.DeleteHrDesignation(
                    HrDesignation,
                    lgUser);

                response.ResponseStatus = 1;
                response.ResponseMessage =
                    "Designation deleted successfully";
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
