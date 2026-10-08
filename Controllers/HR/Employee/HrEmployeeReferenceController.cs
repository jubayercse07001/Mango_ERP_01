using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using ERP.DAL;
using ERP.DAL.HR.Common;
using ERP.Models.User;
using ERP.Models.HR.Employee.HrEmployeeReference;
using ERP.Models.HR.Employee.HrEmployeeReferenceReference;

namespace ERP.Controllers.HR.Employee
{
    public class HrEmployeeReferenceController : Controller
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

            // Note: Passed empty string or standard employee identification context filter if desired
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
                return Redirect(Url.Action("Index", "HrEmployeeReference"));

            ViewBag.User = user;

            return View();
        }

        [HttpPost]
        public ActionResult Add(HrEmployeeReferenceInfo HrEmployeeReference)
        {
            HrEmployeeReferenceResponse response = new HrEmployeeReferenceResponse();

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

            // Input Model Validation Rules
            if (string.IsNullOrWhiteSpace(HrEmployeeReference.EmployeeId))
                response.ResponseMessages.Add("Employee ID is required");

            if (string.IsNullOrWhiteSpace(HrEmployeeReference.ReferenceName))
                response.ResponseMessages.Add("Reference Name is required");

            if (string.IsNullOrWhiteSpace(HrEmployeeReference.PresentAddress))
                response.ResponseMessages.Add("Present Address is required");

            else if (HrEmployeeReferenceDAL.GetHrEmployeeReferenceByName(HrEmployeeReference.ReferenceName, null) != null)
                response.ResponseMessages.Add("Reference Name already exists");

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
                HrEmployeeReference.RowId = Guid.NewGuid().ToString();

                HrEmployeeReferenceDAL.AddHrEmployeeReference(HrEmployeeReference, lgUser);
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
            response.ResponseMessage = "Employee reference added successfully";

            return Json(response);
        }

        public ActionResult GetHrEmployeeReferenceItemList()
        {
            List<HrEmployeeReferenceInfo> list = new List<HrEmployeeReferenceInfo>();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Json(list, JsonRequestBehavior.AllowGet);

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Json(list, JsonRequestBehavior.AllowGet);

            HrEmployeeReferenceResponse resp =
                HrEmployeeReferenceDAL.GetFilteredHrEmployeeReferences(Request);

            return Json(new
            {
                data = resp.HrEmployeeReferences,
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
                return Redirect(Url.Action("Index", "HrEmployeeReference"));

            HrEmployeeReferenceInfo reference =
                HrEmployeeReferenceDAL.GetEmployeeReferenceByID(RouteData.Values["id"].ToString());

            if (reference == null)
                return Redirect(Url.Action("Index", "HrEmployeeReference"));

            ViewBag.User = user;
            ViewBag.HrEmployeeReference = reference;

            return View();
        }

        [HttpPost]
        public ActionResult Edit(HrEmployeeReferenceInfo HrEmployeeReference)
        {
            HrEmployeeReferenceResponse response = new HrEmployeeReferenceResponse();

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

            // Input Model Validation Rules
            if (string.IsNullOrWhiteSpace(HrEmployeeReference.EmployeeId))
                response.ResponseMessages.Add("Employee ID is required");

            if (string.IsNullOrWhiteSpace(HrEmployeeReference.ReferenceName))
                response.ResponseMessages.Add("Reference Name is required");

            if (string.IsNullOrWhiteSpace(HrEmployeeReference.PresentAddress))
                response.ResponseMessages.Add("Present Address is required");

            else if (HrEmployeeReferenceDAL.GetHrEmployeeReferenceByName(
                HrEmployeeReference.ReferenceName,
                HrEmployeeReference.RowId) != null)
                response.ResponseMessages.Add("Reference Name already exists");

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
                HrEmployeeReferenceDAL.UpdateHrEmployeeReference(HrEmployeeReference, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;

                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "Employee reference updated successfully";

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
                return Redirect(Url.Action("Index", "HrEmployeeReference"));

            HrEmployeeReferenceInfo reference =
                HrEmployeeReferenceDAL.GetEmployeeReferenceByID(RouteData.Values["id"].ToString());

            if (reference == null)
                return Redirect(Url.Action("Index", "HrEmployeeReference"));

            ViewBag.User = user;
            ViewBag.HrEmployeeReference = reference;

            return View();
        }

        [HttpPost]
        public ActionResult Delete(HrEmployeeReferenceInfo HrEmployeeReference)
        {
            HrEmployeeReferenceResponse response = new HrEmployeeReferenceResponse();

            string lgUser = Session["LoggedInUser"]?.ToString();

            try
            {
                HrEmployeeReferenceDAL.DeleteHrEmployeeReference(HrEmployeeReference, lgUser);

                response.ResponseStatus = 1;
                response.ResponseMessage = "Employee reference deleted successfully";
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