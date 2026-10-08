using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using ERP.DAL;
using ERP.DAL.Inventory;

using ERP.Models.User;
using ERP.Models.Inventory.InvDepartment;
using ERP.DAL.Inventory.InvProperty;

namespace ERP.Controllers.Inventory
{
    public class InvDepartmentController : Controller
    {
        // ============================================================
        // INDEX
        // ============================================================
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

            // List permission
            if (user.PermissionGroup[0] != '1')
                return Redirect(Url.Action("Index", "Home"));

            ViewBag.InvDepartments =
                InvDepartmentDAL.GetAllInvDepartments();

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

            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Redirect(Url.Action("Logout", "Login"));

            // Add permission
            if (user.PermissionGroup[1] != '1')
                return Redirect(
                    Url.Action("Index", "InvDepartment"));

            ViewBag.User = user;

            return View();
        }


        // ============================================================
        // ADD - POST
        // ============================================================
        [HttpPost]
        public ActionResult Add(
            InvDepartmentInfo InvDepartment)
        {
            InvDepartmentResponse response =
                new InvDepartmentResponse();

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

            // --------------------------------------------------------
            // Permission validation
            // --------------------------------------------------------
            if (user.PermissionGroup[1] != '1')
            {
                response.ResponseStatus = -2;
                response.ResponseCode = "Not Authorized";

                return Json(response);
            }


            // --------------------------------------------------------
            // Department Name validation
            // --------------------------------------------------------
            if (string.IsNullOrWhiteSpace(
                InvDepartment.DepartmentName))
            {
                response.ResponseMessages.Add(
                    "Department Name is required");
            }
            else if (
                InvDepartmentDAL.GetInvDepartmentByName(
                    InvDepartment.DepartmentName,
                    null) != null)
            {
                response.ResponseMessages.Add(
                    "Department Name already exists");
            }


            // --------------------------------------------------------
            // Short Department Name validation
            // --------------------------------------------------------
            if (string.IsNullOrWhiteSpace(
                InvDepartment.ShortDeptName))
            {
                response.ResponseMessages.Add(
                    "Short Department Name is required");
            }


            // --------------------------------------------------------
            // Validation response
            // --------------------------------------------------------
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
                InvDepartment.RowId =
                    Guid.NewGuid().ToString();

                InvDepartmentDAL.AddInvDepartment(
                    InvDepartment,
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
                "Department added successfully";

            return Json(response);
        }


        // ============================================================
        // DATATABLE ITEM LIST
        // ============================================================
        public ActionResult GetInvDepartmentItemList()
        {
            List<InvDepartmentInfo> list =
                new List<InvDepartmentInfo>();

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

            
            InvDepartmentDAL.InvDepartmentResponse resp = InvDepartmentDAL.GetFilteredInvDepartments(Request);


            return Json(
                new
                {
                    data = resp.InvDepartments,
                    draw = Request["draw"],
                    recordsTotal =
                        resp.TotalRecordCount,
                    recordsFiltered =
                        resp.FilteredRecordCount
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

            // Edit permission
            if (user.PermissionGroup[2] != '1')
                return Redirect(
                    Url.Action("Index", "InvDepartment"));


            string rowId =
                RouteData.Values["id"]?.ToString();

            if (string.IsNullOrEmpty(rowId))
                return Redirect(
                    Url.Action("Index", "InvDepartment"));


            InvDepartmentInfo department =
                InvDepartmentDAL.GetDepartmentByID(rowId);

            if (department == null)
                return Redirect(
                    Url.Action("Index", "InvDepartment"));


            ViewBag.User = user;
            ViewBag.InvDepartment = department;

            return View();
        }


        // ============================================================
        // EDIT - POST
        // ============================================================
        [HttpPost]
        public ActionResult Edit(
            InvDepartmentInfo InvDepartment)
        {
            InvDepartmentResponse response =
                new InvDepartmentResponse();

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


            // --------------------------------------------------------
            // Permission validation
            // --------------------------------------------------------
            if (user.PermissionGroup[2] != '1')
            {
                response.ResponseStatus = -2;
                response.ResponseCode = "Not Authorized";

                return Json(response);
            }


            // --------------------------------------------------------
            // Department Name validation
            // --------------------------------------------------------
            if (string.IsNullOrWhiteSpace(
                InvDepartment.DepartmentName))
            {
                response.ResponseMessages.Add(
                    "Department Name is required");
            }
            else if (
                InvDepartmentDAL.GetInvDepartmentByName(
                    InvDepartment.DepartmentName,
                    InvDepartment.RowId) != null)
            {
                response.ResponseMessages.Add(
                    "Department Name already exists");
            }


            // --------------------------------------------------------
            // Short Department Name validation
            // --------------------------------------------------------
            if (string.IsNullOrWhiteSpace(
                InvDepartment.ShortDeptName))
            {
                response.ResponseMessages.Add(
                    "Short Department Name is required");
            }


            // --------------------------------------------------------
            // Validation response
            // --------------------------------------------------------
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
                InvDepartmentDAL.UpdateInvDepartment(
                    InvDepartment,
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
                "Department updated successfully";

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


            // Delete permission
            if (user.PermissionGroup[3] != '1')
                return Redirect(
                    Url.Action("Index", "InvDepartment"));


            string rowId =
                RouteData.Values["id"]?.ToString();

            if (string.IsNullOrEmpty(rowId))
                return Redirect(
                    Url.Action("Index", "InvDepartment"));


            InvDepartmentInfo department =
                InvDepartmentDAL.GetDepartmentByID(rowId);

            if (department == null)
                return Redirect(
                    Url.Action("Index", "InvDepartment"));


            ViewBag.User = user;
            ViewBag.InvDepartment = department;

            return View();
        }


        // ============================================================
        // DELETE - POST
        // ============================================================
        [HttpPost]
        public ActionResult Delete(
            InvDepartmentInfo InvDepartment)
        {
            InvDepartmentResponse response =
                new InvDepartmentResponse();

            string lgUser =
                Session["LoggedInUser"]?.ToString();


            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                response.ResponseCode = "Not Logged In";

                return Json(response);
            }


            try
            {
                UserInfo user =
                    UserDAL.GetUserByUserCode(lgUser);

                if (user == null)
                {
                    response.ResponseStatus = -1;
                    response.ResponseCode = "Invalid User";

                    return Json(response);
                }


                if (user.PermissionGroup[3] != '1')
                {
                    response.ResponseStatus = -2;
                    response.ResponseCode = "Not Authorized";

                    return Json(response);
                }


                InvDepartmentDAL.DeleteInvDepartment(
                    InvDepartment,
                    lgUser);

                response.ResponseStatus = 1;
                response.ResponseCode = "Success";
                response.ResponseMessage =
                    "Department deleted successfully";
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