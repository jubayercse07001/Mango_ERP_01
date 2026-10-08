using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ERP.Models.HR.Organization.HrMainDivision;
using ERP.Models.User;
using ERP.DAL.HR.Organization;
using ERP.DAL;

namespace ERP.Controllers.HR.Organization
{
    public class HrMainDivisionController : Controller
    {
        // GET: HrMainDivision

        public ActionResult Index()
        {
            string lgUser = string.Empty;

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
            {
                return Redirect(Url.Action("Logout", "Login"));
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
            {
                return Redirect(Url.Action("Logout", "Login"));
            }

            if (user.PermissionGroup[0] != '1')
            {
                return Redirect(Url.Action("Index", "Home"));
            }

            ViewBag.HrMainDivisions =
                HrMainDivisionDAL.GetAllHrMainDivisions();

            ViewBag.User = user;

            return View();
        }

        [HttpGet]
        public ActionResult Add()
        {
            string lgUser = string.Empty;

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
            {
                return Redirect(Url.Action("Logout", "Login"));
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
            {
                return Redirect(Url.Action("Logout", "Login"));
            }

            if (user.PermissionGroup[1] != '1')
            {
                return Redirect(Url.Action("Index", "HrMainDivision"));
            }

            ViewBag.User = user;

            ViewBag.HrMainDivisions =
                HrMainDivisionDAL.GetAllHrMainDivisions();

            return View();
        }

        [HttpPost]
        public ActionResult Add(HrMainDivisionInfo HrMainDivision)
        {
            HrMainDivisionResponse response =
                new HrMainDivisionResponse();

            string lgUser = string.Empty;

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

            HrMainDivision.RowId = Guid.NewGuid().ToString();

            if (string.IsNullOrWhiteSpace(HrMainDivision.MainDivisionName))
            {
                response.ResponseMessages.Add(
                    "Main Division Name is required");
            }
            else if (HrMainDivisionDAL.GetHrMainDivisionByName(
                HrMainDivision.MainDivisionName,
                HrMainDivision.RowId) != null)
            {
                response.ResponseMessages.Add(
                    "Main Division Name already exists");
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
                HrMainDivisionDAL.AddHrMainDivision(
                    HrMainDivision,
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
                "Main Division is added successfully";

            response.HrMainDivision = HrMainDivision;

            return Json(response);
        }

        public ActionResult GetHrMainDivisionItemList()
        {
            List<HrMainDivisionInfo> HrMainDivisionItems =
                new List<HrMainDivisionInfo>();

            string lgUser = string.Empty;

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
            {
                return Json(
                    HrMainDivisionItems,
                    JsonRequestBehavior.AllowGet);
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
            {
                return Json(
                    HrMainDivisionItems,
                    JsonRequestBehavior.AllowGet);
            }

            if (user.PermissionGroup[0] != '1')
            {
                return Json(
                    HrMainDivisionItems,
                    JsonRequestBehavior.AllowGet);
            }

            HrMainDivisionResponse resp =
                HrMainDivisionDAL.GetFilteredHrMainDivisions(Request);

            return Json(new
            {
                data = resp.HrMainDivisions,
                draw = Request["draw"],
                recordsTotal = resp.TotalRecordCount,
                recordsFiltered = resp.FilteredRecordCount

            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Edit()
        {
            string lgUser = string.Empty;

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
            {
                return Redirect(Url.Action("Logout", "Login"));
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
            {
                return Redirect(Url.Action("Logout", "Login"));
            }

            if (user.PermissionGroup[2] != '1')
            {
                return Redirect(Url.Action("Index", "HrMainDivision"));
            }

            HrMainDivisionInfo HrMainDivision = HrMainDivisionDAL.GetMainDivisionByID(RouteData.Values["id"].ToString());

            if (HrMainDivision == null)
            {
                return Redirect(Url.Action("Index", "HrMainDivision"));
            }

            ViewBag.User = user;
            ViewBag.HrMainDivision = HrMainDivision;
            ViewBag.HrMainDivisions = HrMainDivisionDAL.GetAllHrMainDivisions();

            // ADD THIS LINE: Fetch all designations for the multi-select pool
            // (Replace 'HrDesignationDAL.GetAllDesignations()' with your actual DAL method name if it differs)
            ViewBag.DesignationList = HrDesignationDAL.GetAllHrDesignations();

            return View();
        }

        [HttpPost]
        public ActionResult Edit(HrMainDivisionInfo HrMainDivision)
        {
            HrMainDivisionResponse response =
                new HrMainDivisionResponse();

            string lgUser = string.Empty;

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

            if (user.PermissionGroup[2] != '1')
            {
                response.ResponseStatus = -2;

                response.ResponseCode = "Not Authorized";

                return Json(response);
            }

            if (string.IsNullOrWhiteSpace(
                HrMainDivision.MainDivisionName))
            {
                response.ResponseMessages.Add(
                    "Main Division Name is required");
            }
            else if (HrMainDivisionDAL.GetHrMainDivisionByName(
                HrMainDivision.MainDivisionName,
                HrMainDivision.RowId) != null)
            {
                response.ResponseMessages.Add(
                    "Main Division Name already exists");
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
                HrMainDivisionDAL.UpdateHrMainDivision(
                    HrMainDivision,
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
                "Main Division is updated successfully";

            response.HrMainDivision = HrMainDivision;

            return Json(response);
        }

        [HttpGet]
        public ActionResult Delete()
        {
            string lgUser = string.Empty;

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
            {
                return Redirect(Url.Action("Logout", "Login"));
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
            {
                return Redirect(Url.Action("Logout", "Login"));
            }

            if (user.PermissionGroup[3] != '1')
            {
                return Redirect(
                    Url.Action("Index", "HrMainDivision"));
            }

            HrMainDivisionInfo HrMainDivision =
                HrMainDivisionDAL.GetMainDivisionByID(
                    RouteData.Values["id"].ToString());

            if (HrMainDivision == null)
            {
                return Redirect(
                    Url.Action("Index", "HrMainDivision"));
            }

            ViewBag.User = user;

            ViewBag.HrMainDivision = HrMainDivision;

            return View();
        }

        [HttpPost]
        public ActionResult Delete(HrMainDivisionInfo HrMainDivision)
        {
            HrMainDivisionResponse response =
                new HrMainDivisionResponse();

            string lgUser = string.Empty;

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

            if (user.PermissionGroup[3] != '1')
            {
                response.ResponseStatus = -2;

                response.ResponseCode = "Not Authorized";

                return Json(response);
            }

            try
            {
                HrMainDivisionDAL.DeleteHrMainDivision(
                    HrMainDivision,
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
                "Main Division is deleted successfully";

            response.HrMainDivision = HrMainDivision;

            return Json(response);
        }
    }
}