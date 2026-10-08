using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using ERP.DAL;

using ERP.Models.User;
using ERP.Models.Inventory.InvFamily;
using ERP.DAL.Inventory;

namespace ERP.Controllers.Inventory.InvFamily
{
    public class InvFamilyController : Controller
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

            ViewBag.InvFamilies = InvFamilyDAL.GetAllInvFamilies();
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
                return Redirect(Url.Action("Index", "InvFamily"));

            ViewBag.User = user;

            // Dropdown list for related Class selection
            ViewBag.ClassList = InvClassDAL.GetAllInvClasses();

            return View();
        }

        [HttpPost]
        public ActionResult Add(InvFamilyInfo invFamily)
        {
            InvFamilyResponse response = new InvFamilyResponse();

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

            if (string.IsNullOrWhiteSpace(invFamily.FamilyName))
                response.ResponseMessages.Add("Family Name is required");
            else if (InvFamilyDAL.GetInvFamilyByName(invFamily.FamilyName, null) != null)
                response.ResponseMessages.Add("Family Name already exists");

            if (string.IsNullOrWhiteSpace(invFamily.ClassId))
                response.ResponseMessages.Add("Class is required");

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
                invFamily.RowId = Guid.NewGuid().ToString();

                InvFamilyDAL.AddInvFamily(invFamily, lgUser);
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
            response.ResponseMessage = "Inventory Family added successfully";

            return Json(response);
        }

        public ActionResult GetInvFamilyItemList()
        {
            List<InvFamilyInfo> list = new List<InvFamilyInfo>();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Json(list, JsonRequestBehavior.AllowGet);

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Json(list, JsonRequestBehavior.AllowGet);

            // FIX 1: Use 'var' to infer ERP.DAL.Inventory.InvFamilyDAL.InvFamilyResponse
            var resp = InvFamilyDAL.GetFilteredInvFamilies(Request);

            return Json(new
            {
                // FIX 2: Correct property name from resp.InvFamilys to resp.InvFamilies
                data = resp.InvFamilies,
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
                return Redirect(Url.Action("Index", "InvFamily"));

            InvFamilyInfo family =
                InvFamilyDAL.GetFamilyByID(RouteData.Values["id"].ToString());

            if (family == null)
                return Redirect(Url.Action("Index", "InvFamily"));

            ViewBag.User = user;
            ViewBag.InvFamily = family;

            ViewBag.ClassList = InvClassDAL.GetAllInvClasses();

            return View();
        }

        [HttpPost]
        public ActionResult Edit(InvFamilyInfo invFamily)
        {
            InvFamilyResponse response = new InvFamilyResponse();

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

            if (string.IsNullOrWhiteSpace(invFamily.FamilyName))
                response.ResponseMessages.Add("Family Name is required");
            else if (InvFamilyDAL.GetInvFamilyByName(
                invFamily.FamilyName,
                invFamily.RowId) != null)
                response.ResponseMessages.Add("Family Name already exists");

            if (string.IsNullOrWhiteSpace(invFamily.ClassId))
                response.ResponseMessages.Add("Class is required");

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
                InvFamilyDAL.UpdateInvFamily(invFamily, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;

                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "Inventory Family updated successfully";

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
                return Redirect(Url.Action("Index", "InvFamily"));

            InvFamilyInfo family =
                InvFamilyDAL.GetFamilyByID(RouteData.Values["id"].ToString());

            if (family == null)
                return Redirect(Url.Action("Index", "InvFamily"));

            ViewBag.User = user;
            ViewBag.InvFamily = family;

            return View();
        }

        [HttpPost]
        public ActionResult Delete(InvFamilyInfo invFamily)
        {
            InvFamilyResponse response = new InvFamilyResponse();

            string lgUser = Session["LoggedInUser"]?.ToString();

            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                response.ResponseCode = "Not Logged In";
                return Json(response);
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null || user.PermissionGroup[3] != '1')
            {
                response.ResponseStatus = -2;
                response.ResponseCode = "Not Authorized";
                return Json(response);
            }

            try
            {
                InvFamilyDAL.DeleteInvFamily(invFamily, lgUser);

                response.ResponseStatus = 1;
                response.ResponseMessage = "Inventory Family deleted successfully";
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