using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using ERP.DAL;
using ERP.DAL.HR.Common;
using ERP.DAL.HR.Organization;

using ERP.Models.User;
using ERP.Models.HR.Organization.HrGrade;

namespace ERP.Controllers.HR.Organization
{
    public class HrGradeController : Controller
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

            ViewBag.HrGrades = HrGradeDAL.GetAllHrGrades();
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
                return Redirect(Url.Action("Index", "HrGrade"));

            ViewBag.User = user;

            // Dependent Dropdown requirement for managing Grade contexts
            ViewBag.EmployeeTypeList = HrEmployeeTypeDAL.GetAllHrEmployeeTypes();

            return View();
        }

        [HttpPost]
        public ActionResult Add(HrGradeInfo HrGrade)
        {
            HrGradeResponse response = new HrGradeResponse();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
            {
                response.TotalRecordCount = -1; // Align tracking properties for system status payload anomalies
                // Alternatively, if your data model includes a custom field like ResponseStatus, populate it here.
                return Json(new { ResponseStatus = -1, ResponseCode = "Not Logged In" });
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
            {
                return Json(new { ResponseStatus = -1, ResponseCode = "Invalid User" });
            }

            if (user.PermissionGroup[1] != '1')
            {
                return Json(new { ResponseStatus = -2, ResponseCode = "Not Authorized" });
            }

            List<string> errorMessages = new List<string>();

            if (string.IsNullOrWhiteSpace(HrGrade.GradeName))
            {
                errorMessages.Add("Grade Name is required");
            }
            else if (HrGradeDAL.GetHrGradeByName(HrGrade.GradeName, null) != null)
            {
                errorMessages.Add("Grade Name already exists");
            }

            if (errorMessages.Count > 0)
            {
                string parsedHtmlMessage = "<ul><li>" + errorMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(new { ResponseStatus = 0, ResponseCode = "Invalid Data", ResponseMessage = parsedHtmlMessage });
            }

            try
            {
                HrGrade.RowId = Guid.NewGuid().ToString();

                HrGradeDAL.AddHrGrade(HrGrade, lgUser);
            }
            catch (Exception ex)
            {
                return Json(new { ResponseStatus = 0, ResponseCode = "Database Error", ResponseMessage = ex.Message });
            }

            return Json(new { ResponseStatus = 1, ResponseCode = "Success", ResponseMessage = "Grade added successfully" });
        }

        public ActionResult GetHrGradeItemList()
        {
            List<HrGradeInfo> list = new List<HrGradeInfo>();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Json(list, JsonRequestBehavior.AllowGet);

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Json(list, JsonRequestBehavior.AllowGet);

            ERP.DAL.HR.Organization.HrGradeDAL.HrGradeResponse resp = HrGradeDAL.GetFilteredHrGrades(Request);


            return Json(new
            {
                data = resp.HrGrades,
                draw = Request["draw"],
                recordsTotal = resp.TotalRecordCount,
                recordsFiltered = resp.FilteredRecordCount
            }, JsonRequestBehavior.AllowGet);
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
                return Redirect(Url.Action("Index", "HrGrade"));

            if (RouteData.Values["id"] == null)
                return Redirect(Url.Action("Index", "HrGrade"));

            HrGradeInfo grade = HrGradeDAL.GetGradeByID(RouteData.Values["id"].ToString());

            if (grade == null)
                return Redirect(Url.Action("Index", "HrGrade"));

            ViewBag.User = user;
            ViewBag.HrGrade = grade;

            ViewBag.EmployeeTypeList = HrEmployeeTypeDAL.GetAllHrEmployeeTypes();

            return View();
        }

        [HttpPost]
        public ActionResult Edit(HrGradeInfo HrGrade)
        {
            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
            {
                return Json(new { ResponseStatus = -1 });
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
            {
                return Json(new { ResponseStatus = -1 });
            }

            if (user.PermissionGroup[2] != '1')
            {
                return Json(new { ResponseStatus = -2 });
            }

            List<string> errorMessages = new List<string>();

            if (string.IsNullOrWhiteSpace(HrGrade.GradeName))
            {
                errorMessages.Add("Grade Name is required");
            }
            else if (HrGradeDAL.GetHrGradeByName(HrGrade.GradeName, HrGrade.RowId) != null)
            {
                errorMessages.Add("Grade Name already exists");
            }

            if (errorMessages.Count > 0)
            {
                string parsedHtmlMessage = "<ul><li>" + errorMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(new { ResponseStatus = 0, ResponseCode = "Invalid Data", ResponseMessage = parsedHtmlMessage });
            }

            try
            {
                HrGradeDAL.UpdateHrGrade(HrGrade, lgUser);
            }
            catch (Exception ex)
            {
                return Json(new { ResponseStatus = 0, ResponseCode = "Database Error", ResponseMessage = ex.Message });
            }

            return Json(new { ResponseStatus = 1, ResponseMessage = "Grade updated successfully" });
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
                return Redirect(Url.Action("Index", "HrGrade"));

            if (RouteData.Values["id"] == null)
                return Redirect(Url.Action("Index", "HrGrade"));

            HrGradeInfo grade = HrGradeDAL.GetGradeByID(RouteData.Values["id"].ToString());

            if (grade == null)
                return Redirect(Url.Action("Index", "HrGrade"));

            ViewBag.User = user;
            ViewBag.HrGrade = grade;

            return View();
        }

        [HttpPost]
        public ActionResult Delete(HrGradeInfo HrGrade)
        {
            string lgUser = Session["LoggedInUser"]?.ToString();

            try
            {
                HrGradeDAL.DeleteHrGrade(HrGrade, lgUser);
                return Json(new { ResponseStatus = 1, ResponseMessage = "Grade deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { ResponseStatus = 0, ResponseMessage = ex.Message });
            }
        }
    }
}