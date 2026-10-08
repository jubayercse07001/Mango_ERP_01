using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using ERP.DAL;
using ERP.DAL.HR.Organization;

using ERP.Models.User;
using ERP.Models.HR.Organization.HrDivision;
using ERP.DAL.HR.Common;

namespace ERP.Controllers.HR.Organization
{
    public class HrDivisionController : Controller
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

            ViewBag.HrDivisions = HrDivisionDAL.GetAllHrDivisions();
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
                return Redirect(Url.Action("Index", "HrDivision"));

            ViewBag.User = user;

            // dropdowns
            ViewBag.CompanyList = HrCompanyInformationDAL.GetAllHrCompanyInformations();
            ViewBag.MainDivisionList = HrMainDivisionDAL.GetAllHrMainDivisions();

            return View();
        }

        [HttpPost]
        public ActionResult Add(HrDivisionInfo HrDivision)
        {
            HrDivisionResponse response = new HrDivisionResponse();

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

            if (string.IsNullOrWhiteSpace(HrDivision.DivisionName))
                response.ResponseMessages.Add("Division Name is required");

            else if (HrDivisionDAL.GetHrDivisionByName(HrDivision.DivisionName, null) != null)
                response.ResponseMessages.Add("Division Name already exists");

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
                HrDivision.RowId = Guid.NewGuid().ToString();

                HrDivisionDAL.AddHrDivision(HrDivision, lgUser);
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
            response.ResponseMessage = "Division added successfully";

            return Json(response);
        }

        public ActionResult GetHrDivisionItemList()
        {
            List<HrDivisionInfo> list = new List<HrDivisionInfo>();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Json(list, JsonRequestBehavior.AllowGet);

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Json(list, JsonRequestBehavior.AllowGet);

            HrDivisionResponse resp =
                HrDivisionDAL.GetFilteredHrDivisions(Request);

            return Json(new
            {
                data = resp.HrDivisions,
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
                return Redirect(Url.Action("Index", "HrDivision"));

            HrDivisionInfo division =
                HrDivisionDAL.GetDivisionByID(RouteData.Values["id"].ToString());

            if (division == null)
                return Redirect(Url.Action("Index", "HrDivision"));

            ViewBag.User = user;
            ViewBag.HrDivision = division;

            ViewBag.CompanyList =
                HrCompanyInformationDAL.GetAllHrCompanyInformations();

            ViewBag.MainDivisionList = HrMainDivisionDAL.GetAllHrMainDivisions();

            return View();
        }

        [HttpPost]
        public ActionResult Edit(HrDivisionInfo HrDivision)
        {
            HrDivisionResponse response = new HrDivisionResponse();

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

            if (string.IsNullOrWhiteSpace(HrDivision.DivisionName))
                response.ResponseMessages.Add("Division Name is required");

            else if (HrDivisionDAL.GetHrDivisionByName(
                HrDivision.DivisionName,
                HrDivision.RowId) != null)
                response.ResponseMessages.Add("Division Name already exists");

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
                HrDivisionDAL.UpdateHrDivision(HrDivision, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;

                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "Division updated successfully";

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
                return Redirect(Url.Action("Index", "HrDivision"));

            HrDivisionInfo division =
                HrDivisionDAL.GetDivisionByID(RouteData.Values["id"].ToString());

            if (division == null)
                return Redirect(Url.Action("Index", "HrDivision"));

            ViewBag.User = user;
            ViewBag.HrDivision = division;

            return View();
        }

        [HttpPost]
        public ActionResult Delete(HrDivisionInfo HrDivision)
        {
            HrDivisionResponse response = new HrDivisionResponse();

            string lgUser = Session["LoggedInUser"]?.ToString();

            try
            {
                HrDivisionDAL.DeleteHrDivision(HrDivision, lgUser);

                response.ResponseStatus = 1;
                response.ResponseMessage = "Division deleted successfully";
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