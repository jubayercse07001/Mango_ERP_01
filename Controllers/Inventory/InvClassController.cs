using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using ERP.DAL;
using ERP.DAL.Inventory; 
using ERP.Models.User;
using ERP.Models.Inventory.InvClass;

namespace ERP.Controllers.Inventory
{
    public class InvClassController : Controller
    {
        // GET: InvClass
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

            ViewBag.InvClasses = InvClassDAL.GetAllInvClasses();
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
                return Redirect(Url.Action("Index", "InvClass"));

            ViewBag.User = user;

            // Group Dropdown for selection
            ViewBag.GroupList = InvGroupDAL.GetAllInvGroups();

            return View();
        }

        [HttpPost]
        public ActionResult Add(InvClassInfo invClass)
        {
            InvClassResponse response = new InvClassResponse();

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

            // Validation Rules
            if (string.IsNullOrWhiteSpace(invClass.ClassName))
                response.ResponseMessages.Add("Class Name is required");
            else if (InvClassDAL.GetInvClassByName(invClass.ClassName, null) != null)
                response.ResponseMessages.Add("Class Name already exists");

            if (string.IsNullOrWhiteSpace(invClass.GroupId))
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
                invClass.RowId = Guid.NewGuid().ToString();

                InvClassDAL.AddInvClass(invClass, lgUser);
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
            response.ResponseMessage = "Class added successfully";

            return Json(response);
        }

        public ActionResult GetInvClassItemList()
        {
            List<InvClassInfo> list = new List<InvClassInfo>();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Json(list, JsonRequestBehavior.AllowGet);

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Json(list, JsonRequestBehavior.AllowGet);

            InvClassResponse resp = InvClassDAL.GetFilteredInvClasses(Request);

            return Json(new
            {
                data = resp.InvClasss,
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
                return Redirect(Url.Action("Index", "InvClass"));

            InvClassInfo invClass = InvClassDAL.GetInvClassByID(RouteData.Values["id"].ToString());

            if (invClass == null)
                return Redirect(Url.Action("Index", "InvClass"));

            ViewBag.User = user;
            ViewBag.InvClass = invClass;

            ViewBag.GroupList = InvGroupDAL.GetAllInvGroups();

            return View();
        }

        [HttpPost]
        public ActionResult Edit(InvClassInfo invClass)
        {
            InvClassResponse response = new InvClassResponse();

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

            // Validation Rules
            if (string.IsNullOrWhiteSpace(invClass.ClassName))
                response.ResponseMessages.Add("Class Name is required");
            else if (InvClassDAL.GetInvClassByName(invClass.ClassName, invClass.RowId) != null)
                response.ResponseMessages.Add("Class Name already exists");

            if (string.IsNullOrWhiteSpace(invClass.GroupId))
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
                InvClassDAL.UpdateInvClass(invClass, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;

                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "Class updated successfully";

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
                return Redirect(Url.Action("Index", "InvClass"));

            InvClassInfo invClass = InvClassDAL.GetInvClassByID(RouteData.Values["id"].ToString());

            if (invClass == null)
                return Redirect(Url.Action("Index", "InvClass"));

            ViewBag.User = user;
            ViewBag.InvClass = invClass;

            return View();
        }

        [HttpPost]
        public ActionResult Delete(InvClassInfo invClass)
        {
            InvClassResponse response = new InvClassResponse();

            string lgUser = Session["LoggedInUser"]?.ToString();

            try
            {
                InvClassDAL.DeleteInvClass(invClass, lgUser);

                response.ResponseStatus = 1;
                response.ResponseMessage = "Class deleted successfully";
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