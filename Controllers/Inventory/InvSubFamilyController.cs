using ERP.DAL;
using ERP.DAL.Inventory;
using ERP.Models.Inventory.InvSubFamily;
using ERP.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ERP.Controllers.Inventory
{
    public class InvSubFamilyController : Controller
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

            ViewBag.InvSubFamilies = InvSubFamilyDAL.GetAllInvSubFamilies();
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
                return Redirect(Url.Action("Index", "InvSubFamily"));

            ViewBag.User = user;

            // dropdowns
            ViewBag.FamilyList = InvFamilyDAL.GetAllInvFamilies();

            return View();
        }

        [HttpPost]
        public ActionResult Add(InvSubFamilyInfo invSubFamily)
        {
            InvSubFamilyResponse response = new InvSubFamilyResponse();

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

            if (string.IsNullOrWhiteSpace(invSubFamily.SubFamilyName))
                response.ResponseMessages.Add("Sub Family Name is required");

            else if (InvSubFamilyDAL.GetInvSubFamilyByName(invSubFamily.SubFamilyName, null) != null)
                response.ResponseMessages.Add("Sub Family Name already exists");

            if (string.IsNullOrWhiteSpace(invSubFamily.FamilyId))
                response.ResponseMessages.Add("Family is required");

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
                invSubFamily.RowId = Guid.NewGuid().ToString();

                InvSubFamilyDAL.AddInvSubFamily(invSubFamily, lgUser);
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
            response.ResponseMessage = "Sub Family added successfully";

            return Json(response);
        }

        public ActionResult GetInvSubFamilyItemList()
        {
            List<InvSubFamilyInfo> list = new List<InvSubFamilyInfo>();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Json(list, JsonRequestBehavior.AllowGet);

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Json(list, JsonRequestBehavior.AllowGet);

            InvSubFamilyDAL.InvSubFamilyResponse resp = InvSubFamilyDAL.GetFilteredInvSubFamilies(Request);

            return Json(new
            {
                data = resp.InvSubFamilies,
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
                return Redirect(Url.Action("Index", "InvSubFamily"));

            InvSubFamilyInfo subFamily =
                InvSubFamilyDAL.GetSubFamilyByID(RouteData.Values["id"].ToString());

            if (subFamily == null)
                return Redirect(Url.Action("Index", "InvSubFamily"));

            ViewBag.User = user;
            ViewBag.InvSubFamily = subFamily;

            ViewBag.FamilyList = InvFamilyDAL.GetAllInvFamilies();

            return View();
        }

        [HttpPost]
        public ActionResult Edit(InvSubFamilyInfo invSubFamily)
        {
            InvSubFamilyResponse response = new InvSubFamilyResponse();

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

            if (string.IsNullOrWhiteSpace(invSubFamily.SubFamilyName))
                response.ResponseMessages.Add("Sub Family Name is required");

            else if (InvSubFamilyDAL.GetInvSubFamilyByName(
                invSubFamily.SubFamilyName,
                invSubFamily.RowId) != null)
                response.ResponseMessages.Add("Sub Family Name already exists");

            if (string.IsNullOrWhiteSpace(invSubFamily.FamilyId))
                response.ResponseMessages.Add("Family is required");

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
                InvSubFamilyDAL.UpdateInvSubFamily(invSubFamily, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;

                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "Sub Family updated successfully";

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
                return Redirect(Url.Action("Index", "InvSubFamily"));

            InvSubFamilyInfo subFamily =
                InvSubFamilyDAL.GetSubFamilyByID(RouteData.Values["id"].ToString());

            if (subFamily == null)
                return Redirect(Url.Action("Index", "InvSubFamily"));

            ViewBag.User = user;
            ViewBag.InvSubFamily = subFamily;

            return View();
        }

        [HttpPost]
        public ActionResult Delete(InvSubFamilyInfo invSubFamily)
        {
            InvSubFamilyResponse response = new InvSubFamilyResponse();

            string lgUser = Session["LoggedInUser"]?.ToString();

            try
            {
                InvSubFamilyDAL.DeleteInvSubFamily(invSubFamily, lgUser);

                response.ResponseStatus = 1;
                response.ResponseMessage = "Sub Family deleted successfully";
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