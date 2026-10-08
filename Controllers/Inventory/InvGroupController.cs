using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using ERP.DAL;
using ERP.DAL.Inventory;
using ERP.Models.User;
using ERP.Models.Inventory.InvGroup;

namespace ERP.Controllers.Inventory
{
    public class InvGroupController : Controller
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

            if (user.PermissionGroup[0] != '1')
                return Redirect(Url.Action("Index", "Home"));

            ViewBag.InvGroups = InvGroupDAL.GetAllInvGroups();
            ViewBag.User = user;

            return View();
        }
        [HttpGet]
        public JsonResult GetNextRank()
        {
            int nextRank = InvGroupDAL.GetNextRank();
            return Json(nextRank, JsonRequestBehavior.AllowGet);
        }
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
                return Redirect(Url.Action("Index", "InvGroup"));

            ViewBag.User = user;

            return View();
        }

        [HttpPost]
        public ActionResult Add(InvGroupInfo invGroup)
        {
            InvGroupResponse response = new InvGroupResponse();

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

            // Validations
            if (string.IsNullOrWhiteSpace(invGroup.GroupName))
                response.ResponseMessages.Add("Group Name is required");
            else if (InvGroupDAL.GetInvGroupByName(invGroup.GroupName, null) != null)
                response.ResponseMessages.Add("Group Name already exists");

            if (invGroup.Rank <= 0)
                response.ResponseMessages.Add("Rank must be greater than zero");

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
                invGroup.RowId = Guid.NewGuid().ToString();

                InvGroupDAL.AddInvGroup(invGroup, lgUser);
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
            response.ResponseMessage = "Inventory Group added successfully";

            return Json(response);
        }

        public ActionResult GetInvGroupItemList()
        {
            List<InvGroupInfo> list = new List<InvGroupInfo>();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Json(list, JsonRequestBehavior.AllowGet);

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Json(list, JsonRequestBehavior.AllowGet);

            InvGroupResponse resp = InvGroupDAL.GetFilteredInvGroups(Request);

            return Json(new
            {
                data = resp.InvGroups,
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
                return Redirect(Url.Action("Index", "InvGroup"));

            InvGroupInfo group = InvGroupDAL.GetInvGroupByID(RouteData.Values["id"]?.ToString());

            if (group == null)
                return Redirect(Url.Action("Index", "InvGroup"));

            ViewBag.User = user;
            ViewBag.InvGroup = group;

            return View();
        }

        [HttpPost]
        public ActionResult Edit(InvGroupInfo invGroup)
        {
            InvGroupResponse response = new InvGroupResponse();

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

            // Validations
            if (string.IsNullOrWhiteSpace(invGroup.GroupName))
                response.ResponseMessages.Add("Group Name is required");
            else if (InvGroupDAL.GetInvGroupByName(invGroup.GroupName, invGroup.RowId) != null)
                response.ResponseMessages.Add("Group Name already exists");

            if (invGroup.Rank <= 0)
                response.ResponseMessages.Add("Rank must be greater than zero");

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
                InvGroupDAL.UpdateInvGroup(invGroup, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;

                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "Inventory Group updated successfully";

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
                return Redirect(Url.Action("Index", "InvGroup"));

            InvGroupInfo group = InvGroupDAL.GetInvGroupByID(RouteData.Values["id"]?.ToString());

            if (group == null)
                return Redirect(Url.Action("Index", "InvGroup"));

            ViewBag.User = user;
            ViewBag.InvGroup = group;

            return View();
        }

        [HttpPost]
        public ActionResult Delete(InvGroupInfo invGroup)
        {
            InvGroupResponse response = new InvGroupResponse();

            string lgUser = Session["LoggedInUser"]?.ToString();

            try
            {
                InvGroupDAL.DeleteInvGroup(invGroup, lgUser);

                response.ResponseStatus = 1;
                response.ResponseMessage = "Inventory Group deleted successfully";
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