using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using ERP.DAL;
using ERP.DAL.Inventory;

using ERP.Models.User;
using ERP.Models.Inventory.InvSupplier;
using ERP.DAL.HR.Common;

namespace ERP.Controllers.Inventory
{
    public class InvSupplierController : Controller
    {
        #region Index

        [HttpGet]
        public ActionResult Index()
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

            // View Permission
            if (user.PermissionGroup[0] != '1')
                return Redirect(Url.Action("Index", "Home"));

            ViewBag.User = user;

            ViewBag.InvSuppliers =
                InvSupplierDAL.GetAllInvSuppliers();

            return View();
        }

        #endregion


        #region Add

        [HttpGet]
        public ActionResult Add()
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

            // Add Permission
            if (user.PermissionGroup[1] != '1')
                return Redirect(
                    Url.Action("Index", "InvSupplier"));

            ViewBag.User = user;
            ViewBag.CountryList = HrCountryDAL.GetAllHrCountrys();
            ViewBag.DepartmentList = HrDepartmentDAL.GetAllHrDepartments();
            return View();
        }


        [HttpPost]
        public ActionResult Add(InvSupplierInfo InvSupplier)
        {
            InvSupplierResponse response =
                new InvSupplierResponse();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                response.ResponseCode = "Not Logged In";

                return Json(response);
            }

            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
            {
                response.ResponseStatus = -1;
                response.ResponseCode = "Invalid User";

                return Json(response);
            }

            // Add Permission
            if (user.PermissionGroup[1] != '1')
            {
                response.ResponseStatus = -2;
                response.ResponseCode = "Not Authorized";

                return Json(response);
            }


            #region Validation

            if (string.IsNullOrWhiteSpace(
                InvSupplier.SupplierName))
            {
                response.ResponseMessages.Add(
                    "Supplier Name is required");
            }
            else if (
                InvSupplierDAL.GetInvSupplierByName(
                    InvSupplier.SupplierName,
                    null) != null)
            {
                response.ResponseMessages.Add(
                    "Supplier Name already exists");
            }


            if (string.IsNullOrWhiteSpace(
                InvSupplier.ContactPerson))
            {
                response.ResponseMessages.Add(
                    "Contact Person is required");
            }


            if (string.IsNullOrWhiteSpace(
                InvSupplier.Mobile))
            {
                response.ResponseMessages.Add(
                    "Mobile is required");
            }


            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";

                response.ResponseMessage =
                    "<ul><li>" +
                    response.ResponseMessages.Aggregate(
                        (i, j) =>
                            i + "</li><li>" + j) +
                    "</li></ul>";

                return Json(response);
            }

            #endregion


            try
            {
                InvSupplier.RowId =
                    Guid.NewGuid().ToString();

                InvSupplierDAL.AddInvSupplier(
                    InvSupplier,
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
                "Supplier added successfully";

            return Json(response);
        }

        #endregion


        #region DataTables

        public ActionResult GetInvSupplierItemList()
        {
            List<InvSupplierInfo> list =
                new List<InvSupplierInfo>();

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


            InvSupplierDAL.InvSupplierResponse resp =
                InvSupplierDAL.GetFilteredInvSuppliers(
                    Request);


            return Json(
                new
                {
                    data = resp.InvSuppliers,
                    draw = Request["draw"],
                    recordsTotal = resp.TotalRecordCount,
                    recordsFiltered =
                        resp.FilteredRecordCount
                },
                JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region Edit

        [HttpGet]
        public ActionResult Edit()
        {
            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Redirect(
                    Url.Action("Logout", "Login"));

            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Redirect(
                    Url.Action("Logout", "Login"));

            // Edit Permission
            if (user.PermissionGroup[2] != '1')
                return Redirect(
                    Url.Action("Index", "InvSupplier"));


            string rowId =
                RouteData.Values["id"]?.ToString();

            if (string.IsNullOrEmpty(rowId))
                return Redirect(
                    Url.Action("Index", "InvSupplier"));


            InvSupplierInfo supplier =
                InvSupplierDAL.GetInvSupplierByID(
                    rowId);

            if (supplier == null)
                return Redirect(
                    Url.Action("Index", "InvSupplier"));


            ViewBag.User = user;

            ViewBag.InvSupplier = supplier;
            ViewBag.CountryList = HrCountryDAL.GetAllHrCountrys();
            ViewBag.DepartmentList = HrDepartmentDAL.GetAllHrDepartments();

            return View();
        }


        [HttpPost]
        public ActionResult Edit(
            InvSupplierInfo InvSupplier)
        {
            InvSupplierResponse response =
                new InvSupplierResponse();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                response.ResponseCode =
                    "Not Logged In";

                return Json(response);
            }


            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
            {
                response.ResponseStatus = -1;
                response.ResponseCode =
                    "Invalid User";

                return Json(response);
            }


            // Edit Permission
            if (user.PermissionGroup[2] != '1')
            {
                response.ResponseStatus = -2;
                response.ResponseCode =
                    "Not Authorized";

                return Json(response);
            }


            #region Validation

            if (string.IsNullOrWhiteSpace(
                InvSupplier.SupplierName))
            {
                response.ResponseMessages.Add(
                    "Supplier Name is required");
            }
            else if (
                InvSupplierDAL.GetInvSupplierByName(
                    InvSupplier.SupplierName,
                    InvSupplier.RowId) != null)
            {
                response.ResponseMessages.Add(
                    "Supplier Name already exists");
            }


            if (string.IsNullOrWhiteSpace(
                InvSupplier.ContactPerson))
            {
                response.ResponseMessages.Add(
                    "Contact Person is required");
            }


            if (string.IsNullOrWhiteSpace(
                InvSupplier.Mobile))
            {
                response.ResponseMessages.Add(
                    "Mobile is required");
            }


            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode =
                    "Invalid Data";

                response.ResponseMessage =
                    "<ul><li>" +
                    response.ResponseMessages.Aggregate(
                        (i, j) =>
                            i + "</li><li>" + j) +
                    "</li></ul>";

                return Json(response);
            }

            #endregion


            try
            {
                InvSupplierDAL.UpdateInvSupplier(
                    InvSupplier,
                    lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode =
                    "Database Error";

                response.ResponseMessage =
                    ex.Message;

                return Json(response);
            }


            response.ResponseStatus = 1;
            response.ResponseCode = "Success";
            response.ResponseMessage =
                "Supplier updated successfully";

            return Json(response);
        }

        #endregion


        #region Delete

        [HttpGet]
        public ActionResult Delete()
        {
            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Redirect(
                    Url.Action("Logout", "Login"));

            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Redirect(
                    Url.Action("Logout", "Login"));

            // Delete Permission
            if (user.PermissionGroup[3] != '1')
                return Redirect(
                    Url.Action("Index", "InvSupplier"));


            string rowId =
                RouteData.Values["id"]?.ToString();

            if (string.IsNullOrEmpty(rowId))
                return Redirect(
                    Url.Action("Index", "InvSupplier"));


            InvSupplierInfo supplier =
                InvSupplierDAL.GetInvSupplierByID(
                    rowId);

            if (supplier == null)
                return Redirect(
                    Url.Action("Index", "InvSupplier"));


            ViewBag.User = user;

            ViewBag.InvSupplier = supplier;

            return View();
        }


        [HttpPost]
        public ActionResult Delete(
            InvSupplierInfo InvSupplier)
        {
            InvSupplierResponse response =
                new InvSupplierResponse();

            string lgUser =
                Session["LoggedInUser"]?.ToString();


            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                response.ResponseCode =
                    "Not Logged In";

                return Json(response);
            }


            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
            {
                response.ResponseStatus = -1;
                response.ResponseCode =
                    "Invalid User";

                return Json(response);
            }


            // Delete Permission
            if (user.PermissionGroup[3] != '1')
            {
                response.ResponseStatus = -2;
                response.ResponseCode =
                    "Not Authorized";

                return Json(response);
            }


            try
            {
                InvSupplierDAL.DeleteInvSupplier(
                    InvSupplier,
                    lgUser);

                response.ResponseStatus = 1;
                response.ResponseCode =
                    "Success";

                response.ResponseMessage =
                    "Supplier deleted successfully";
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode =
                    "Database Error";

                response.ResponseMessage =
                    ex.Message;
            }


            return Json(response);
        }

        #endregion


        #region Response

        public class InvSupplierResponse
        {
            public int ResponseStatus { get; set; }

            public string ResponseCode { get; set; }

            public string ResponseMessage { get; set; }

            public List<string> ResponseMessages { get; set; }

            public InvSupplierResponse()
            {
                ResponseMessages =
                    new List<string>();
            }
        }

        #endregion
    }
}