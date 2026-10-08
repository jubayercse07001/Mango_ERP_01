using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using ERP.DAL;
using ERP.DAL.HR.Common;
using ERP.DAL.HR.Organization;

using ERP.Models.User;
using ERP.Models.HR.Organization.HrDepartment;

namespace ERP.Controllers.HR.Organization
{
    public class HrDepartmentController : Controller
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

            ViewBag.HrDepartments = HrDepartmentDAL.GetAllHrDepartments();
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
                return Redirect(Url.Action("Index", "HrDepartment"));

            ViewBag.User = user;

            ViewBag.DepartmentGroupList =
                HrDepartmentGroupDAL.GetAllHrDepartmentGroups();

            ViewBag.BranchList =
                HrBranchDAL.GetAllHrBranches();

            ViewBag.DivisionList =
                HrDivisionDAL.GetAllHrDivisions();

            return View();
        }

        [HttpPost]
        public ActionResult Add(HrDepartmentInfo HrDepartment)
        {
            HrDepartmentResponse response =
                new HrDepartmentResponse();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                response.ResponseCode = "Not Logged In";

                return Json(response);
            }

            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

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

            if (string.IsNullOrWhiteSpace(
                HrDepartment.DepartmentName))
            {
                response.ResponseMessages.Add(
                    "Department Name is required");
            }
            else if (HrDepartmentDAL.GetHrDepartmentByName(
                HrDepartment.DepartmentName,
                null) != null)
            {
                response.ResponseMessages.Add(
                    "Department Name already exists");
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
                HrDepartment.RowId =
                    Guid.NewGuid().ToString();

                HrDepartmentDAL.AddHrDepartment(
                    HrDepartment,
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
                "Department added successfully";

            return Json(response);
        }

        public ActionResult GetHrDepartmentItemList()
        {
            List<HrDepartmentInfo> list =
                new List<HrDepartmentInfo>();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Json(
                    list,
                    JsonRequestBehavior.AllowGet);

            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Json(
                    list,
                    JsonRequestBehavior.AllowGet);

            HrDepartmentResponse resp =
                HrDepartmentDAL.GetFilteredHrDepartments(
                    Request);

            return Json(new
            {
                data = resp.HrDepartments,
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

            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Redirect(Url.Action("Logout", "Login"));

            if (user.PermissionGroup[2] != '1')
                return Redirect(
                    Url.Action("Index", "HrDepartment"));

            HrDepartmentInfo department =
                HrDepartmentDAL.GetDepartmentByID(
                    RouteData.Values["id"].ToString());

            if (department == null)
                return Redirect(
                    Url.Action("Index", "HrDepartment"));

            ViewBag.User = user;
            ViewBag.HrDepartment = department;

            ViewBag.DepartmentGroupList =
                HrDepartmentGroupDAL.GetAllHrDepartmentGroups();

            ViewBag.BranchList =
                HrBranchDAL.GetAllHrBranches();

            ViewBag.DivisionList =
                HrDivisionDAL.GetAllHrDivisions();

            return View();

        }
        
        [HttpPost]
        public ActionResult Edit(HrDepartmentInfo HrDepartment)
        {
            HrDepartmentResponse response =
                new HrDepartmentResponse();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                return Json(response);
            }

            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

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
                HrDepartment.DepartmentName))
            {
                response.ResponseMessages.Add(
                    "Department Name is required");
            }
            else if (HrDepartmentDAL.GetHrDepartmentByName(
                HrDepartment.DepartmentName,
                HrDepartment.RowId) != null)
            {
                response.ResponseMessages.Add(
                    "Department Name already exists");
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
                HrDepartmentDAL.UpdateHrDepartment(
                    HrDepartment,
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
                "Department updated successfully";

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

            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Redirect(Url.Action("Logout", "Login"));

            if (user.PermissionGroup[3] != '1')
                return Redirect(
                    Url.Action("Index", "HrDepartment"));

            HrDepartmentInfo department =
                HrDepartmentDAL.GetDepartmentByID(
                    RouteData.Values["id"].ToString());

            if (department == null)
                return Redirect(
                    Url.Action("Index", "HrDepartment"));

            ViewBag.User = user;
            ViewBag.HrDepartment = department;

            return View();
        }

        [HttpPost]
        public ActionResult Delete(HrDepartmentInfo HrDepartment)
        {
            HrDepartmentResponse response =
                new HrDepartmentResponse();

            string lgUser =
                Session["LoggedInUser"]?.ToString();

            try
            {
                HrDepartmentDAL.DeleteHrDepartment(
                    HrDepartment,
                    lgUser);

                response.ResponseStatus = 1;
                response.ResponseMessage =
                    "Department deleted successfully";
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseMessage = ex.Message;
            }

            return Json(response);
        }
        public JsonResult GetBranchByDivision(string DivisionId)
        {
            var branchList =
                HrBranchDAL.GetBranchByDivisionId(DivisionId);

            return Json(
                branchList,
                JsonRequestBehavior.AllowGet
            );
        }

    }
}