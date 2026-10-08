using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ERP.DAL;
using ERP.DAL.HR.Common;
using ERP.Models.HR.Employee.HrEmployeeEducation;
using ERP.Models.User;
using Mango_ERP.Models;

namespace ERP.Controllers.HR.Common
{
    public class HrEmployeeEducationController : Controller
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

            ViewBag.HrEmployeeEducations = HrEmployeeEducationDAL.GetAllHrEmployeeEducations();
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
                return Redirect(Url.Action("Index", "HrEmployeeEducation"));

            ViewBag.User = user;

            // Lookups for Dropdowns
            // Adjust these DAL calls if your lookups have slightly different names
            ViewBag.EducationalDegreeList = HrEducationalDegreeDAL.GetAllHrEducationalDegrees();
            ViewBag.EducationalInstituteList = HrEducationalInstituteDAL.GetAllHrEducationalInstitutes();

            return View();
        }

        [HttpPost]
        public ActionResult Add(HrEmployeeEducationInfo HrEmployeeEducation)
        {
            HrEmployeeEducationResponse response = new HrEmployeeEducationResponse();

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

            // Input Validations
            if (string.IsNullOrWhiteSpace(HrEmployeeEducation.EmployeeId))
                response.ResponseMessages.Add("Employee ID is required");

            if (string.IsNullOrWhiteSpace(HrEmployeeEducation.EducationalDegreeId))
                response.ResponseMessages.Add("Educational Degree is required");

            // Check for duplicate combination of Employee + Degree if applicable
            if (!string.IsNullOrWhiteSpace(HrEmployeeEducation.EmployeeId) && !string.IsNullOrWhiteSpace(HrEmployeeEducation.EducationalDegreeId))
            {
                if (HrEmployeeEducationDAL.GetHrEmployeeEducationDuplicate(HrEmployeeEducation.EmployeeId, HrEmployeeEducation.EducationalDegreeId, null) != null)
                {
                    response.ResponseMessages.Add("This educational qualification record already exists for the employee");
                }
            }

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
                HrEmployeeEducation.RowId = Guid.NewGuid().ToString();

                HrEmployeeEducationDAL.AddHrEmployeeEducation(HrEmployeeEducation, lgUser);
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
            response.ResponseMessage = "Education record added successfully";

            return Json(response);
        }

        public ActionResult GetHrEmployeeEducationItemList()
        {
            List<HrEmployeeEducationInfo> list = new List<HrEmployeeEducationInfo>();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Json(list, JsonRequestBehavior.AllowGet);

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Json(list, JsonRequestBehavior.AllowGet);

            HrEmployeeEducationResponse resp =
                HrEmployeeEducationDAL.GetFilteredHrEmployeeEducations(Request);

            return Json(new
            {
                data = resp.HrEmployeeEducations,
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
                return Redirect(Url.Action("Index", "HrEmployeeEducation"));

            HrEmployeeEducationInfo education =
                HrEmployeeEducationDAL.GetEducationByID(RouteData.Values["id"].ToString());

            if (education == null)
                return Redirect(Url.Action("Index", "HrEmployeeEducation"));

            ViewBag.User = user;
            ViewBag.HrEmployeeEducation = education;

            // Lookups for Dropdowns
            ViewBag.EducationalDegreeList = HrEducationalDegreeDAL.GetAllHrEducationalDegrees();
            ViewBag.EducationalInstituteList = HrEducationalInstituteDAL.GetAllHrEducationalInstitutes();

            return View();
        }

        [HttpPost]
        public ActionResult Edit(HrEmployeeEducationInfo HrEmployeeEducation)
        {
            HrEmployeeEducationResponse response = new HrEmployeeEducationResponse();

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

            // Input Validations
            if (string.IsNullOrWhiteSpace(HrEmployeeEducation.EmployeeId))
                response.ResponseMessages.Add("Employee ID is required");

            if (string.IsNullOrWhiteSpace(HrEmployeeEducation.EducationalDegreeId))
                response.ResponseMessages.Add("Educational Degree is required");

            // Duplicate qualification check variant for edits
            if (!string.IsNullOrWhiteSpace(HrEmployeeEducation.EmployeeId) && !string.IsNullOrWhiteSpace(HrEmployeeEducation.EducationalDegreeId))
            {
                if (HrEmployeeEducationDAL.GetHrEmployeeEducationDuplicate(HrEmployeeEducation.EmployeeId, HrEmployeeEducation.EducationalDegreeId, HrEmployeeEducation.RowId) != null)
                {
                    response.ResponseMessages.Add("This educational qualification record already exists for the employee");
                }
            }

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
                HrEmployeeEducationDAL.UpdateHrEmployeeEducation(HrEmployeeEducation, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;

                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "Education record updated successfully";

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
                return Redirect(Url.Action("Index", "HrEmployeeEducation"));

            HrEmployeeEducationInfo education =
                HrEmployeeEducationDAL.GetEducationByID(RouteData.Values["id"].ToString());

            if (education == null)
                return Redirect(Url.Action("Index", "HrEmployeeEducation"));

            ViewBag.User = user;
            ViewBag.HrEmployeeEducation = education;

            return View();
        }

        [HttpPost]
        public ActionResult Delete(HrEmployeeEducationInfo HrEmployeeEducation)
        {
            HrEmployeeEducationResponse response = new HrEmployeeEducationResponse();

            string lgUser = Session["LoggedInUser"]?.ToString();

            try
            {
                HrEmployeeEducationDAL.DeleteHrEmployeeEducation(HrEmployeeEducation, lgUser);

                response.ResponseStatus = 1;
                response.ResponseMessage = "Education record deleted successfully";
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