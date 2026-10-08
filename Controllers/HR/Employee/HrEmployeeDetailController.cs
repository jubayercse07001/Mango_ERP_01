using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ERP.DAL;
using ERP.DAL.HR.Common;
using ERP.Models.HR.Employee.EmployeeDetail;
using ERP.Models.User;
using Mango_ERP.Models;

namespace ERP.Controllers.HR.Common
{
    public class HrEmployeeDetailController : Controller
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

            ViewBag.HrEmployeeDetails = HrEmployeeDetailDAL.GetAllHrEmployeeDetails();
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
                return Redirect(Url.Action("Index", "HrEmployeeDetail"));

            ViewBag.User = user;

            // Optional: If you need Dropdown population lists for countries/states later, 
            // you can assign them to ViewBags here similar to HrDivisionController.

            return View();
        }

        [HttpPost]
        public ActionResult Add(HrEmployeeDetailInfo HrEmployeeDetail)
        {
            HrEmployeeDetailResponse response = new HrEmployeeDetailResponse();
            response.ResponseMessages = new List<string>();

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

            // Validation Logic
            if (string.IsNullOrWhiteSpace(HrEmployeeDetail.EmployeeId))
                response.ResponseMessages.Add("Employee ID is required");

            else if (HrEmployeeDetailDAL.GetHrEmployeeDetailByEmployeeId(HrEmployeeDetail.EmployeeId, null) != null)
                response.ResponseMessages.Add("Employee Details for this Employee ID already exist");

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";
                response.ResponseMessage =
                    "<ul><li>" +
                    response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j)
                    + "</li></ul>";

                return Json(response);
            }

            try
            {
                HrEmployeeDetail.RowId = Guid.NewGuid().ToString();

                HrEmployeeDetailDAL.AddHrEmployeeDetail(HrEmployeeDetail, lgUser);
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
            response.ResponseMessage = "Employee details added successfully";

            return Json(response);
        }

        public ActionResult GetHrEmployeeDetailItemList()
        {
            List<HrEmployeeDetailInfo> list = new List<HrEmployeeDetailInfo>();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Json(list, JsonRequestBehavior.AllowGet);

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Json(list, JsonRequestBehavior.AllowGet);

            HrEmployeeDetailResponse resp =
                HrEmployeeDetailDAL.GetFilteredHrEmployeeDetails(Request);

            return Json(new
            {
                data = resp.HrEmployeeDetails,
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
                return Redirect(Url.Action("Index", "HrEmployeeDetail"));

            HrEmployeeDetailInfo employeeDetail =
                HrEmployeeDetailDAL.GetEmployeeDetailByID(RouteData.Values["id"].ToString());

            if (employeeDetail == null)
                return Redirect(Url.Action("Index", "HrEmployeeDetail"));

            ViewBag.User = user;
            ViewBag.HrEmployeeDetail = employeeDetail;

            return View();
        }

        [HttpPost]
        public ActionResult Edit(HrEmployeeDetailInfo HrEmployeeDetail)
        {
            HrEmployeeDetailResponse response = new HrEmployeeDetailResponse();
            response.ResponseMessages = new List<string>();

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

            // Validation Logic
            if (string.IsNullOrWhiteSpace(HrEmployeeDetail.EmployeeId))
                response.ResponseMessages.Add("Employee ID is required");

            else if (HrEmployeeDetailDAL.GetHrEmployeeDetailByEmployeeId(
                HrEmployeeDetail.EmployeeId,
                HrEmployeeDetail.RowId) != null)
                response.ResponseMessages.Add("Employee Details for this Employee ID already exist");

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";

                response.ResponseMessage =
                    "<ul><li>" +
                    response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j)
                    + "</li></ul>";

                return Json(response);
            }

            try
            {
                HrEmployeeDetailDAL.UpdateHrEmployeeDetail(HrEmployeeDetail, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;

                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "Employee details updated successfully";

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
                return Redirect(Url.Action("Index", "HrEmployeeDetail"));

            HrEmployeeDetailInfo employeeDetail =
                HrEmployeeDetailDAL.GetEmployeeDetailByID(RouteData.Values["id"].ToString());

            if (employeeDetail == null)
                return Redirect(Url.Action("Index", "HrEmployeeDetail"));

            ViewBag.User = user;
            ViewBag.HrEmployeeDetail = employeeDetail;

            return View();
        }

        [HttpPost]
        public ActionResult Delete(HrEmployeeDetailInfo HrEmployeeDetail)
        {
            HrEmployeeDetailResponse response = new HrEmployeeDetailResponse();

            string lgUser = Session["LoggedInUser"]?.ToString();

            try
            {
                HrEmployeeDetailDAL.DeleteHrEmployeeDetail(HrEmployeeDetail, lgUser);

                response.ResponseStatus = 1;
                response.ResponseMessage = "Employee details deleted successfully";
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