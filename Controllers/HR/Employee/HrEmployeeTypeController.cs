using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using ERP.DAL;
using ERP.DAL.HR.Organization;

using ERP.Models.User;
using ERP.Models.HR.Organization.HrEmployeeType;
using ERP.Models.HR.Employee;

namespace ERP.Controllers.HR.Organization
{
    public class HrEmployeeTypeController : Controller
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

            ViewBag.HrEmployeeTypes =
                HrEmployeeTypeDAL.GetAllHrEmployeeTypes();

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
                return Redirect(Url.Action("Index", "HrEmployeeType"));

            ViewBag.User = user;

            return View();
        }

        [HttpPost]
        public ActionResult Add(HrEmployeeTypeInfo HrEmployeeType)
        {
            HrEmployeeTypeResponse response = new HrEmployeeTypeResponse();

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

            if (string.IsNullOrWhiteSpace(
                HrEmployeeType.EmployeeTypeName))
            {
                response.ResponseMessages.Add(
                    "Employee Type Name is required");
            }
            else if (
                HrEmployeeTypeDAL.GetHrEmployeeTypeByName(
                    HrEmployeeType.EmployeeTypeName,
                    null) != null)
            {
                response.ResponseMessages.Add(
                    "Employee Type Name already exists");
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
                HrEmployeeType.RowId =
                    Guid.NewGuid().ToString();

                HrEmployeeTypeDAL.AddHrEmployeeType(
                    HrEmployeeType,
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
                "Employee Type added successfully";

            return Json(response);
        }

        public ActionResult GetHrEmployeeTypeItemList()
        {
            List<HrEmployeeTypeInfo> list =
                new List<HrEmployeeTypeInfo>();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Json(list, JsonRequestBehavior.AllowGet);

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Json(list, JsonRequestBehavior.AllowGet);

            HrEmployeeTypeResponse resp =
                HrEmployeeTypeDAL.GetFilteredHrEmployeeTypes(
                    Request);

            return Json(new
            {
                data = resp.HrEmployeeTypes,
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
                    Url.Action("Index", "HrEmployeeType"));

            HrEmployeeTypeInfo employeeType =
                HrEmployeeTypeDAL.GetEmployeeTypeByID(
                    RouteData.Values["id"].ToString());

            if (employeeType == null)
                return Redirect(
                    Url.Action("Index", "HrEmployeeType"));

            ViewBag.User = user;
            ViewBag.HrEmployeeType = employeeType;

            return View();
        }

        [HttpPost]
        public ActionResult Edit(
            HrEmployeeTypeInfo HrEmployeeType)
        {
            HrEmployeeTypeResponse response =
                new HrEmployeeTypeResponse();

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
                HrEmployeeType.EmployeeTypeName))
            {
                response.ResponseMessages.Add(
                    "Employee Type Name is required");
            }
            else if (
                HrEmployeeTypeDAL.GetHrEmployeeTypeByName(
                    HrEmployeeType.EmployeeTypeName,
                    HrEmployeeType.RowId) != null)
            {
                response.ResponseMessages.Add(
                    "Employee Type Name already exists");
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
                HrEmployeeTypeDAL.UpdateHrEmployeeType(
                    HrEmployeeType,
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
                "Employee Type updated successfully";

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
                    Url.Action("Index", "HrEmployeeType"));

            HrEmployeeTypeInfo employeeType =
                HrEmployeeTypeDAL.GetEmployeeTypeByID(
                    RouteData.Values["id"].ToString());

            if (employeeType == null)
                return Redirect(
                    Url.Action("Index", "HrEmployeeType"));

            ViewBag.User = user;
            ViewBag.HrEmployeeType = employeeType;

            return View();
        }

        [HttpPost]
        public ActionResult Delete(
            HrEmployeeTypeInfo HrEmployeeType)
        {
            HrEmployeeTypeResponse response =
                new HrEmployeeTypeResponse();

            string lgUser =
                Session["LoggedInUser"]?.ToString();

            try
            {
                HrEmployeeTypeDAL.DeleteHrEmployeeType(
                    HrEmployeeType,
                    lgUser);

                response.ResponseStatus = 1;
                response.ResponseMessage =
                    "Employee Type deleted successfully";
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