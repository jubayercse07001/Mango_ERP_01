using ERP.DAL;
using ERP.DAL.Inventory;
using ERP.Models.Inventory.InvItemCategory;
using ERP.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ERP.Controllers.Inventory
{
    public class InvItemCategoryController : Controller
    {
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

            // Permission index 0 for View/Index access
            if (user.PermissionGroup == null || user.PermissionGroup.Length < 1 || user.PermissionGroup[0] != '1')
                return Redirect(Url.Action("Index", "Home"));

            ViewBag.InvItemCategories = InvItemCategoryDAL.GetAllInvItemCategories();
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

            // Permission index 1 for Add access
            if (user.PermissionGroup == null || user.PermissionGroup.Length < 2 || user.PermissionGroup[1] != '1')
                return Redirect(Url.Action("Index", "InvItemCategory"));

            ViewBag.User = user;

            return View();
        }

        [HttpPost]
        public ActionResult Add(InvItemCategoryInfo category)
        {
            InvItemCategoryResponse response = new InvItemCategoryResponse();

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

            if (user.PermissionGroup == null || user.PermissionGroup.Length < 2 || user.PermissionGroup[1] != '1')
            {
                response.ResponseStatus = -2;
                response.ResponseCode = "Not Authorized";
                return Json(response);
            }

            // Input Validation
            if (string.IsNullOrWhiteSpace(category.CategoryName))
                response.ResponseMessages.Add("Category Name is required");
            else if (InvItemCategoryDAL.GetInvItemCategoryByName(category.CategoryName, null) != null)
                response.ResponseMessages.Add("Category Name already exists");

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
                category.RowId = Guid.NewGuid().ToString();

                InvItemCategoryDAL.AddInvItemCategory(category, lgUser);
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
            response.ResponseMessage = "Item category added successfully";

            return Json(response);
        }

        public ActionResult GetInvItemCategoryList()
        {
            List<InvItemCategoryInfo> list = new List<InvItemCategoryInfo>();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Json(list, JsonRequestBehavior.AllowGet);

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Json(list, JsonRequestBehavior.AllowGet);

            var resp = InvItemCategoryDAL.GetFilteredInvItemCategories(Request);

            return Json(new
            {
                data = resp.InvItemCategorys,
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

            // Permission index 2 for Edit access
            if (user.PermissionGroup == null || user.PermissionGroup.Length < 3 || user.PermissionGroup[2] != '1')
                return Redirect(Url.Action("Index", "InvItemCategory"));

            string rowId = RouteData.Values["id"] != null ? RouteData.Values["id"].ToString() : "";
            InvItemCategoryInfo category = InvItemCategoryDAL.GetInvItemCategoryByID(rowId);

            if (category == null)
                return Redirect(Url.Action("Index", "InvItemCategory"));

            ViewBag.User = user;
            ViewBag.InvItemCategory = category;

            return View();
        }

        [HttpPost]
        public ActionResult Edit(InvItemCategoryInfo category)
        {
            InvItemCategoryResponse response = new InvItemCategoryResponse();

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

            if (user.PermissionGroup == null || user.PermissionGroup.Length < 3 || user.PermissionGroup[2] != '1')
            {
                response.ResponseStatus = -2;
                return Json(response);
            }

            // Input Validation
            if (string.IsNullOrWhiteSpace(category.CategoryName))
                response.ResponseMessages.Add("Category Name is required");
            else if (InvItemCategoryDAL.GetInvItemCategoryByName(category.CategoryName, category.RowId) != null)
                response.ResponseMessages.Add("Category Name already exists");

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
                InvItemCategoryDAL.UpdateInvItemCategory(category, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;

                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "Item category updated successfully";

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

            // Permission index 3 for Delete access
            if (user.PermissionGroup == null || user.PermissionGroup.Length < 4 || user.PermissionGroup[3] != '1')
                return Redirect(Url.Action("Index", "InvItemCategory"));

            string rowId = RouteData.Values["id"] != null ? RouteData.Values["id"].ToString() : "";
            InvItemCategoryInfo category = InvItemCategoryDAL.GetInvItemCategoryByID(rowId);

            if (category == null)
                return Redirect(Url.Action("Index", "InvItemCategory"));

            ViewBag.User = user;
            ViewBag.InvItemCategory = category;

            return View();
        }

        [HttpPost]
        public ActionResult Delete(InvItemCategoryInfo category)
        {
            InvItemCategoryResponse response = new InvItemCategoryResponse();

            string lgUser = Session["LoggedInUser"]?.ToString();

            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                return Json(response);
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null || user.PermissionGroup == null || user.PermissionGroup.Length < 4 || user.PermissionGroup[3] != '1')
            {
                response.ResponseStatus = -2;
                return Json(response);
            }

            try
            {
                InvItemCategoryDAL.DeleteInvItemCategory(category, lgUser);

                response.ResponseStatus = 1;
                response.ResponseMessage = "Item category deleted successfully";
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseMessage = ex.Message;
            }

            return Json(response);
        }
    }

    // Helper class for returning JSON responses to AJAX requests
    public class InvItemCategoryResponse
    {
        public int ResponseStatus { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public List<string> ResponseMessages { get; set; }

        public InvItemCategoryResponse()
        {
            ResponseMessages = new List<string>();
        }
    }
}