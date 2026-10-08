using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using ERP.DAL;
using ERP.DAL.Inventory.InvProperty; // Update path if your DAL namespace differs
using ERP.Models.User;
using ERP.Models.Inventory.InvProperty;
using ERP.DAL.Inventory;

namespace ERP.Controllers.Inventory.InvProperty
{
    public class InvPropertyController : Controller
    {
        // GET: InvProperty
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

            ViewBag.InvProperties = InvPropertyDAL.GetAllInvProperties();
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
                return Redirect(Url.Action("Index", "InvProperty"));

            ViewBag.User = user;

            // Dropdowns (e.g., Property Group list)

            ViewBag.GroupList = InvGroupDAL.GetAllInvGroups();

            return View();
        }

        [HttpPost]
        public ActionResult Add(InvPropertyInfo InvProperty)
        {
            InvPropertyResponse response = new InvPropertyResponse();

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

            // Validation
            if (string.IsNullOrWhiteSpace(InvProperty.PropertyName))
                response.ResponseMessages.Add("Property Name is required");
            else if (InvPropertyDAL.GetInvPropertyByName(InvProperty.PropertyName, null) != null)
                response.ResponseMessages.Add("Property Name already exists");

            if (string.IsNullOrWhiteSpace(InvProperty.GroupId))
                response.ResponseMessages.Add("Group is required");

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
                InvProperty.RowId = Guid.NewGuid().ToString();

                InvPropertyDAL.AddInvProperty(InvProperty, lgUser);
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
            response.ResponseMessage = "Property added successfully";

            return Json(response);
        }

        public ActionResult GetInvPropertyItemList()
        {
            List<InvPropertyInfo> list = new List<InvPropertyInfo>();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Json(list, JsonRequestBehavior.AllowGet);

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Json(list, JsonRequestBehavior.AllowGet);
            var resp = InvPropertyDAL.GetFilteredInvProperties(Request);

            return Json(new
            {
                data = resp.InvProperties,
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
                return Redirect(Url.Action("Index", "InvProperty"));

            InvPropertyInfo property =
                InvPropertyDAL.GetPropertyByID(RouteData.Values["id"].ToString());

            if (property == null)
                return Redirect(Url.Action("Index", "InvProperty"));

            ViewBag.User = user;
            ViewBag.InvProperty = property;
            ViewBag.GroupList = InvGroupDAL.GetAllInvGroups();

            return View();
        }

        [HttpPost]
        public ActionResult Edit(InvPropertyInfo InvProperty)
        {
            InvPropertyResponse response = new InvPropertyResponse();

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

            // Validation
            if (string.IsNullOrWhiteSpace(InvProperty.PropertyName))
                response.ResponseMessages.Add("Property Name is required");
            else if (InvPropertyDAL.GetInvPropertyByName(
                InvProperty.PropertyName,
                InvProperty.RowId) != null)
                response.ResponseMessages.Add("Property Name already exists");

            if (string.IsNullOrWhiteSpace(InvProperty.GroupId))
                response.ResponseMessages.Add("Group is required");

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
                InvPropertyDAL.UpdateInvProperty(InvProperty, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;

                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "Property updated successfully";

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
                return Redirect(Url.Action("Index", "InvProperty"));

            InvPropertyInfo property =
                InvPropertyDAL.GetPropertyByID(RouteData.Values["id"].ToString());

            if (property == null)
                return Redirect(Url.Action("Index", "InvProperty"));

            ViewBag.User = user;
            ViewBag.InvProperty = property;

            return View();
        }

        [HttpPost]
        public ActionResult Delete(InvPropertyInfo InvProperty)
        {
            InvPropertyResponse response = new InvPropertyResponse();

            string lgUser = Session["LoggedInUser"]?.ToString();

            try
            {
                InvPropertyDAL.DeleteInvProperty(InvProperty, lgUser);

                response.ResponseStatus = 1;
                response.ResponseMessage = "Property deleted successfully";
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