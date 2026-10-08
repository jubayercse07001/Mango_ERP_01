using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ERP.DAL;
using ERP.DAL.Inventory;
using ERP.Models.User;
using ERP.Models.Inventory.InvManufacturer;

namespace ERP.Controllers.Inventory
{
    public class InvManufacturerController : Controller
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
            ViewBag.InvManufacturers = InvManufacturerDAL.GetAllInvManufacturers();
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
                return Redirect(Url.Action("Index", "InvManufacturer"));
            ViewBag.User = user;
            return View();
        }

        [HttpPost]
        public ActionResult Add(InvManufacturerInfo InvManufacturer)
        {
            InvManufacturerResponse response = new InvManufacturerResponse();
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
            if (string.IsNullOrWhiteSpace(InvManufacturer.ManufacturerName))
                response.ResponseMessages.Add("Manufacturer Name is required");
            else if (InvManufacturerDAL.GetInvManufacturerByName(InvManufacturer.ManufacturerName, null) != null)
                response.ResponseMessages.Add("Manufacturer Name already exists");
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
                InvManufacturer.RowId = Guid.NewGuid().ToString();
                InvManufacturerDAL.AddInvManufacturer(InvManufacturer, lgUser);
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
            response.ResponseMessage = "Manufacturer added successfully";
            return Json(response);
        }

        public ActionResult GetInvManufacturerItemList()
        {
            List<InvManufacturerInfo> list = new List<InvManufacturerInfo>();
            string lgUser = "";
            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();
            if (string.IsNullOrEmpty(lgUser))
                return Json(list, JsonRequestBehavior.AllowGet);
            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null)
                return Json(list, JsonRequestBehavior.AllowGet);

            
            var resp = InvManufacturerDAL.GetFilteredInvManufacturers(Request);

            return Json(new
            {
                data = resp.InvManufacturers,
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
                return Redirect(Url.Action("Index", "InvManufacturer"));
            InvManufacturerInfo manufacturer =
                InvManufacturerDAL.GetManufacturerByID(RouteData.Values["id"].ToString());
            if (manufacturer == null)
                return Redirect(Url.Action("Index", "InvManufacturer"));
            ViewBag.User = user;
            ViewBag.InvManufacturer = manufacturer;
            return View();
        }

        [HttpPost]
        public ActionResult Edit(InvManufacturerInfo InvManufacturer)
        {
            InvManufacturerResponse response = new InvManufacturerResponse();
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
            if (string.IsNullOrWhiteSpace(InvManufacturer.ManufacturerName))
                response.ResponseMessages.Add("Manufacturer Name is required");
            else if (InvManufacturerDAL.GetInvManufacturerByName(
                InvManufacturer.ManufacturerName,
                InvManufacturer.RowId) != null)
                response.ResponseMessages.Add("Manufacturer Name already exists");
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
                InvManufacturerDAL.UpdateInvManufacturer(InvManufacturer, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;
                return Json(response);
            }
            response.ResponseStatus = 1;
            response.ResponseMessage = "Manufacturer updated successfully";
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
                return Redirect(Url.Action("Index", "InvManufacturer"));
            InvManufacturerInfo manufacturer =
                InvManufacturerDAL.GetManufacturerByID(RouteData.Values["id"].ToString());
            if (manufacturer == null)
                return Redirect(Url.Action("Index", "InvManufacturer"));
            ViewBag.User = user;
            ViewBag.InvManufacturer = manufacturer;
            return View();
        }

        [HttpPost]
        public ActionResult Delete(InvManufacturerInfo InvManufacturer)
        {
            InvManufacturerResponse response = new InvManufacturerResponse();
            string lgUser = Session["LoggedInUser"]?.ToString();
            try
            {
                InvManufacturerDAL.DeleteInvManufacturer(InvManufacturer, lgUser);
                response.ResponseStatus = 1;
                response.ResponseMessage = "Manufacturer deleted successfully";
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