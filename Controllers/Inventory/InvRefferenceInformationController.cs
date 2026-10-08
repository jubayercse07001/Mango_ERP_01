using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using ERP.DAL;
using ERP.DAL.Inventory;
using ERP.Models.User;
using ERP.Models.Inventory.InvRefferenceInformation;

namespace ERP.Controllers.Inventory
{
    public class InvRefferenceInformationController : Controller
    {
        // ============================================================
        // INDEX
        // ============================================================
        [HttpGet]
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

            // PermissionGroup[0] = View
            if (user.PermissionGroup[0] != '1')
                return Redirect(Url.Action("Index", "Home"));

            ViewBag.User = user;

            return View();
        }


        // ============================================================
        // ADD - GET
        // ============================================================
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

            // PermissionGroup[1] = Add
            if (user.PermissionGroup[1] != '1')
                return Redirect(
                    Url.Action("Index", "InvRefferenceInformation"));

            ViewBag.User = user;

            return View();
        }


        // ============================================================
        // ADD - POST
        // ============================================================
        [HttpPost]
        public ActionResult Add(InvRefferenceInformationInfo obj)
        {
            InvRefferenceInformationResponse response =
                new InvRefferenceInformationResponse();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            // --------------------------------------------------------
            // Login validation
            // --------------------------------------------------------
            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                response.ResponseCode = "Not Logged In";
                response.ResponseMessage = "User is not logged in.";

                return Json(response);
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
            {
                response.ResponseStatus = -1;
                response.ResponseCode = "Invalid User";
                response.ResponseMessage = "Invalid user.";

                return Json(response);
            }

            // PermissionGroup[1] = Add
            if (user.PermissionGroup[1] != '1')
            {
                response.ResponseStatus = -2;
                response.ResponseCode = "Not Authorized";
                response.ResponseMessage =
                    "You are not authorized to add Reference Information.";

                return Json(response);
            }

            // --------------------------------------------------------
            // Validation
            // --------------------------------------------------------
            if (string.IsNullOrWhiteSpace(obj.RefferenceName))
            {
                response.ResponseMessages.Add(
                    "Reference Name is required");
            }

            else
            {
                InvRefferenceInformationInfo existing =
                    InvRefferenceInformationDAL
                    .GetInvRefferenceInformationByName(
                        obj.RefferenceName,
                        null);

                if (existing != null)
                {
                    response.ResponseMessages.Add(
                        "Reference Name already exists");
                }
            }

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";

                response.ResponseMessage =
                    "<ul><li>" +
                    response.ResponseMessages.Aggregate(
                        (i, j) => i + "</li><li>" + j) +
                    "</li></ul>";

                return Json(response);
            }

            // --------------------------------------------------------
            // Save
            // --------------------------------------------------------
            try
            {
                obj.RowId = Guid.NewGuid().ToString();

                InvRefferenceInformationDAL
                    .AddInvRefferenceInformation(obj, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;

                return Json(response);
            }

            // --------------------------------------------------------
            // Success
            // --------------------------------------------------------
            response.ResponseStatus = 1;
            response.ResponseCode = "Success";
            response.ResponseMessage =
                "Reference Information added successfully";

            return Json(response);
        }


        // ============================================================
        // GET LIST - DATATABLE
        // ============================================================
        [HttpGet]
        public ActionResult GetInvRefferenceInformationItemList()
        {
            List<InvRefferenceInformationInfo> list =
                new List<InvRefferenceInformationInfo>();

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

            var resp =
                InvRefferenceInformationDAL
                .GetFilteredInvRefferenceInformations(Request);

            return Json(
                new
                {
                    data = resp.InvRefferenceInformations,
                    draw = Request["draw"],
                    recordsTotal = resp.TotalRecordCount,
                    recordsFiltered = resp.FilteredRecordCount
                },
                JsonRequestBehavior.AllowGet);
        }


        // ============================================================
        // EDIT - GET
        // ============================================================
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

            // PermissionGroup[2] = Edit
            if (user.PermissionGroup[2] != '1')
                return Redirect(
                    Url.Action(
                        "Index",
                        "InvRefferenceInformation"));

            string id =
                RouteData.Values["id"]?.ToString();

            if (string.IsNullOrEmpty(id))
                return Redirect(
                    Url.Action(
                        "Index",
                        "InvRefferenceInformation"));

            InvRefferenceInformationInfo obj =
                InvRefferenceInformationDAL
                .GetInvRefferenceInformationByID(id);

            if (obj == null)
                return Redirect(
                    Url.Action(
                        "Index",
                        "InvRefferenceInformation"));

            ViewBag.User = user;
            ViewBag.InvRefferenceInformation = obj;

            return View();
        }


        // ============================================================
        // EDIT - POST
        // ============================================================
        [HttpPost]
        public ActionResult Edit(
            InvRefferenceInformationInfo obj)
        {
            InvRefferenceInformationResponse response =
                new InvRefferenceInformationResponse();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            // --------------------------------------------------------
            // Login validation
            // --------------------------------------------------------
            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                response.ResponseCode = "Not Logged In";
                response.ResponseMessage =
                    "User is not logged in.";

                return Json(response);
            }

            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
            {
                response.ResponseStatus = -1;
                response.ResponseCode = "Invalid User";
                response.ResponseMessage =
                    "Invalid user.";

                return Json(response);
            }

            // PermissionGroup[2] = Edit
            if (user.PermissionGroup[2] != '1')
            {
                response.ResponseStatus = -2;
                response.ResponseCode = "Not Authorized";
                response.ResponseMessage =
                    "You are not authorized to edit Reference Information.";

                return Json(response);
            }

            // --------------------------------------------------------
            // Validation
            // --------------------------------------------------------
            if (string.IsNullOrWhiteSpace(obj.RefferenceName))
            {
                response.ResponseMessages.Add(
                    "Reference Name is required");
            }
            else
            {
                InvRefferenceInformationInfo existing =
                    InvRefferenceInformationDAL
                    .GetInvRefferenceInformationByName(
                        obj.RefferenceName,
                        obj.RowId);

                if (existing != null)
                {
                    response.ResponseMessages.Add(
                        "Reference Name already exists");
                }
            }

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";

                response.ResponseMessage =
                    "<ul><li>" +
                    response.ResponseMessages.Aggregate(
                        (i, j) => i + "</li><li>" + j) +
                    "</li></ul>";

                return Json(response);
            }

            // --------------------------------------------------------
            // Update
            // --------------------------------------------------------
            try
            {
                InvRefferenceInformationDAL
                    .UpdateInvRefferenceInformation(
                        obj,
                        lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;

                return Json(response);
            }

            // --------------------------------------------------------
            // Success
            // --------------------------------------------------------
            response.ResponseStatus = 1;
            response.ResponseCode = "Success";
            response.ResponseMessage =
                "Reference Information updated successfully";

            return Json(response);
        }


        // ============================================================
        // DELETE - GET
        // ============================================================
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

            // PermissionGroup[3] = Delete
            if (user.PermissionGroup[3] != '1')
                return Redirect(
                    Url.Action(
                        "Index",
                        "InvRefferenceInformation"));

            string id =
                RouteData.Values["id"]?.ToString();

            if (string.IsNullOrEmpty(id))
                return Redirect(
                    Url.Action(
                        "Index",
                        "InvRefferenceInformation"));

            InvRefferenceInformationInfo obj =
                InvRefferenceInformationDAL
                .GetInvRefferenceInformationByID(id);

            if (obj == null)
                return Redirect(
                    Url.Action(
                        "Index",
                        "InvRefferenceInformation"));

            ViewBag.User = user;
            ViewBag.InvRefferenceInformation = obj;

            return View();
        }


        // ============================================================
        // DELETE - POST
        // ============================================================
        [HttpPost]
        public ActionResult Delete(
            InvRefferenceInformationInfo obj)
        {
            InvRefferenceInformationResponse response =
                new InvRefferenceInformationResponse();

            string lgUser =
                Session["LoggedInUser"]?.ToString();

            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                response.ResponseCode = "Not Logged In";
                response.ResponseMessage =
                    "User is not logged in.";

                return Json(response);
            }

            try
            {
                InvRefferenceInformationDAL
                    .DeleteInvRefferenceInformation(
                        obj,
                        lgUser);

                response.ResponseStatus = 1;
                response.ResponseCode = "Success";
                response.ResponseMessage =
                    "Reference Information deleted successfully";
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;
            }

            return Json(response);
        }
    }
}