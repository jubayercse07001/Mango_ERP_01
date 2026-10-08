using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ERP.DAL;
using ERP.DAL.HR.Common;
using ERP.Models.HR.Employee.HrEmployeeExperience;
using ERP.Models.User;
using Mango_ERP.Models;

namespace ERP.Controllers.HR.Common
{
    public class HrEmployeeExperienceController : Controller
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

            ViewBag.HrEmployeeExperiences = HrEmployeeExperienceDAL.GetAllHrEmployeeExperiences();
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
                return Redirect(Url.Action("Index", "HrEmployeeExperience"));

            ViewBag.User = user;

            return View();
        }

        [HttpPost]
        public ActionResult Add(HrEmployeeExperienceInfo HrEmployeeExperience)
        {
            HrEmployeeExperienceResponse response = new HrEmployeeExperienceResponse();

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

            // Input Validation Rules
            if (string.IsNullOrWhiteSpace(HrEmployeeExperience.EmployeeId))
                response.ResponseMessages.Add("Employee ID is required");

            if (string.IsNullOrWhiteSpace(HrEmployeeExperience.CompanyName))
                response.ResponseMessages.Add("Company Name is required");

            // Check for duplicate experience record (Same Employee working at the same Company)
            if (!string.IsNullOrWhiteSpace(HrEmployeeExperience.EmployeeId) && !string.IsNullOrWhiteSpace(HrEmployeeExperience.CompanyName))
            {
                if (HrEmployeeExperienceDAL.GetHrEmployeeExperienceDuplicate(HrEmployeeExperience.EmployeeId, HrEmployeeExperience.CompanyName, null) != null)
                {
                    response.ResponseMessages.Add("This company experience record already exists for the employee");
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
                HrEmployeeExperience.RowId = Guid.NewGuid().ToString();

                HrEmployeeExperienceDAL.AddHrEmployeeExperience(HrEmployeeExperience, lgUser);
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
            response.ResponseMessage = "Experience record added successfully";

            return Json(response);
        }

        public ActionResult GetHrEmployeeExperienceItemList()
        {
            List<HrEmployeeExperienceInfo> list = new List<HrEmployeeExperienceInfo>();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Json(list, JsonRequestBehavior.AllowGet);

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Json(list, JsonRequestBehavior.AllowGet);

            HrEmployeeExperienceResponse resp =
                HrEmployeeExperienceDAL.GetFilteredHrEmployeeExperiences(Request);

            return Json(new
            {
                data = resp.HrEmployeeExperiences,
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
                return Redirect(Url.Action("Index", "HrEmployeeExperience"));

            HrEmployeeExperienceInfo experience =
                HrEmployeeExperienceDAL.GetExperienceByID(RouteData.Values["id"].ToString());

            if (experience == null)
                return Redirect(Url.Action("Index", "HrEmployeeExperience"));

            ViewBag.User = user;
            ViewBag.HrEmployeeExperience = experience;

            return View();
        }

        [HttpPost]
        public ActionResult Edit(HrEmployeeExperienceInfo HrEmployeeExperience)
        {
            HrEmployeeExperienceResponse response = new HrEmployeeExperienceResponse();

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

            // Input Validation Rules
            if (string.IsNullOrWhiteSpace(HrEmployeeExperience.EmployeeId))
                response.ResponseMessages.Add("Employee ID is required");

            if (string.IsNullOrWhiteSpace(HrEmployeeExperience.CompanyName))
                response.ResponseMessages.Add("Company Name is required");

            // Duplicate evaluation tracking variance applied specifically for edits
            if (!string.IsNullOrWhiteSpace(HrEmployeeExperience.EmployeeId) && !string.IsNullOrWhiteSpace(HrEmployeeExperience.CompanyName))
            {
                if (HrEmployeeExperienceDAL.GetHrEmployeeExperienceDuplicate(HrEmployeeExperience.EmployeeId, HrEmployeeExperience.CompanyName, HrEmployeeExperience.RowId) != null)
                {
                    response.ResponseMessages.Add("This company experience record already exists for the employee");
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
                HrEmployeeExperienceDAL.UpdateHrEmployeeExperience(HrEmployeeExperience, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;

                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "Experience record updated successfully";

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
                return Redirect(Url.Action("Index", "HrEmployeeExperience"));

            HrEmployeeExperienceInfo experience =
                HrEmployeeExperienceDAL.GetExperienceByID(RouteData.Values["id"].ToString());

            if (experience == null)
                return Redirect(Url.Action("Index", "HrEmployeeExperience"));

            ViewBag.User = user;
            ViewBag.HrEmployeeExperience = experience;

            return View();
        }

        [HttpPost]
        public ActionResult Delete(HrEmployeeExperienceInfo HrEmployeeExperience)
        {
            HrEmployeeExperienceResponse response = new HrEmployeeExperienceResponse();

            string lgUser = Session["LoggedInUser"]?.ToString();

            try
            {
                HrEmployeeExperienceDAL.DeleteHrEmployeeExperience(HrEmployeeExperience, lgUser);

                response.ResponseStatus = 1;
                response.ResponseMessage = "Experience record deleted successfully";
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