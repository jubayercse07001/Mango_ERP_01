using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ERP.DAL;
using ERP.DAL.Inventory;
using ERP.Models.User;
//using ERP.Models.Inventory.InvUnit.InvUnitInfo;
using ERP.Models.Inventory.InvUnit;

namespace ERP.Controllers.Inventory
{
    public class InvUnitController : Controller
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

            ViewBag.InvUnits = InvUnitDAL.GetAllInvUnits();
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
                return Redirect(Url.Action("Index", "InvUnit"));

            ViewBag.User = user;

            return View();
        }

        [HttpPost]
        public ActionResult Add(InvUnitInfo InvUnit)
        {
            InvUnitResponse response = new InvUnitResponse();

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

            if (string.IsNullOrWhiteSpace(InvUnit.MinUnitName))
                response.ResponseMessages.Add("Unit Name is required");

            else if (InvUnitDAL.GetInvUnitByName(InvUnit.MinUnitName, null) != null)
                response.ResponseMessages.Add("Unit Name already exists");

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
                InvUnit.RowId = Guid.NewGuid().ToString();

                InvUnitDAL.AddInvUnit(InvUnit, lgUser);
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
            response.ResponseMessage = "Unit added successfully";

            return Json(response);
        }

        public ActionResult GetInvUnitItemList()
        {
            List<InvUnitInfo> list = new List<InvUnitInfo>();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Json(list, JsonRequestBehavior.AllowGet);

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Json(list, JsonRequestBehavior.AllowGet);

            InvUnitResponse resp =
                InvUnitDAL.GetFilteredInvUnits(Request);

            return Json(new
            {
                data = resp.InvUnits,
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
                return Redirect(Url.Action("Index", "InvUnit"));

            InvUnitInfo unit =
                InvUnitDAL.GetInvUnitByID(RouteData.Values["id"].ToString());

            if (unit == null)
                return Redirect(Url.Action("Index", "InvUnit"));

            ViewBag.User = user;
            ViewBag.InvUnit = unit;

            return View();
        }

        [HttpPost]
        public ActionResult Edit(InvUnitInfo InvUnit)
        {
            InvUnitResponse response = new InvUnitResponse();

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

            if (string.IsNullOrWhiteSpace(InvUnit.MinUnitName))
                response.ResponseMessages.Add("Unit Name is required");

            else if (InvUnitDAL.GetInvUnitByName(
                InvUnit.MinUnitName,
                InvUnit.RowId) != null)
                response.ResponseMessages.Add("Unit Name already exists");

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
                InvUnitDAL.UpdateInvUnit(InvUnit, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;

                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "Unit updated successfully";

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
                return Redirect(Url.Action("Index", "InvUnit"));

            InvUnitInfo unit =
                InvUnitDAL.GetInvUnitByID(RouteData.Values["id"].ToString());

            if (unit == null)
                return Redirect(Url.Action("Index", "InvUnit"));

            ViewBag.User = user;
            ViewBag.InvUnit = unit;

            return View();
        }

        [HttpPost]
        public ActionResult Delete(InvUnitInfo InvUnit)
        {
            InvUnitResponse response = new InvUnitResponse();

            string lgUser = Session["LoggedInUser"]?.ToString();

            try
            {
                InvUnitDAL.DeleteInvUnit(InvUnit, lgUser);

                response.ResponseStatus = 1;
                response.ResponseMessage = "Unit deleted successfully";
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