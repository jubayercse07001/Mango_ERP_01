using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ERP.DAL;
using ERP.DAL.Inventory;
using ERP.DAL.HR.Organization;
using ERP.Models.User;
using ERP.Models.Inventory.InvDepartmentRequisition;
using ERP.Models.Inventory.InvDepartmentRequisitionDetail;
using ERP.DAL.Inventory.InvJob;
using ERP.DAL.HR.Employee;
using ERP.DAL.HR.Common;

namespace ERP.Controllers.Inventory
{
    public class InvDepartmentRequisition2ndController : Controller
    {
        // PermissionGroup positions (same as the previous controller)
        private const int PERM_VIEW = 0;
        private const int PERM_ADD = 1;
        private const int PERM_EDIT = 2;
        private const int PERM_DELETE = 3;

        // Workflow actions: change these if you have dedicated permission positions
        private const int PERM_FORWARD = PERM_EDIT;
        private const int PERM_APPROVE = PERM_EDIT;
        private const int PERM_REJECT = PERM_EDIT;
        private const int PERM_ISSUE = PERM_EDIT;

        #region Index / List

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

            if (user.PermissionGroup[PERM_VIEW] != '1')
                return Redirect(Url.Action("Index", "Home"));

            ViewBag.User = user;

            // dropdowns for the Advance Search panel
            ViewBag.DepartmentList = HrDepartmentDAL.GetAllHrDepartments();

            return View();
        }

        public ActionResult GetInvDepartmentRequisitionItemList()
        {
            List<InvDepartmentRequisitionInfo> list = new List<InvDepartmentRequisitionInfo>();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Json(list, JsonRequestBehavior.AllowGet);

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Json(list, JsonRequestBehavior.AllowGet);

            InvDepartmentRequisition2ndDAL.InvDepartmentRequisitionResponse resp;
            

            try
            {
                resp = InvDepartmentRequisition2ndDAL.GetFilteredRequisitions(Request, lgUser);
            }
            catch (Exception ex)
            {
                // Surface the real DB error in the JSON payload instead of letting
                // it bubble up as a raw HTML 500 page that DataTables can't parse.
                return Json(new
                {
                    data = new List<InvDepartmentRequisitionInfo>(),
                    draw = Request["draw"],
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    error = ex.Message
                },
                JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                data = resp.Requisitions,
                draw = Request["draw"],
                recordsTotal = resp.TotalRecordCount,
                recordsFiltered = resp.FilteredRecordCount
            },
            JsonRequestBehavior.AllowGet);
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

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Redirect(Url.Action("Logout", "Login"));

            if (user.PermissionGroup[PERM_ADD] != '1')
                return Redirect(Url.Action("Index", "InvDepartmentRequisitionNew"));

            ViewBag.User = user;

            // dropdowns
            ViewBag.DepartmentList = HrDepartmentDAL.GetAllHrDepartments();
            ViewBag.EmployeeList = HrEmployeeDAL.GetAllHrEmployees();
            ViewBag.JobList = InvJobDAL.GetAllInvJobs();
            ViewBag.ItemList = InvItemDAL.GetAllInvItems();
            ViewBag.UnitList = InvUnitDAL.GetAllInvUnits();

            ViewBag.NextRequisitionCode = InvDepartmentRequisition2ndDAL.GetNextRequisitionCode();

            return View();
        }

        //[HttpPost]
        //public ActionResult Add(InvDepartmentRequisitionInfo Requisition, string DetailsJson)
        //{
        //    InvDepartmentRequisitionInfo.InvDepartmentRequisitionResponse response =
        //        new InvDepartmentRequisitionInfo.InvDepartmentRequisitionResponse();

        //    string lgUser = "";

        //    if (Session["LoggedInUser"] != null)
        //        lgUser = Session["LoggedInUser"].ToString();

        //    if (string.IsNullOrEmpty(lgUser))
        //    {
        //        response.ResponseStatus = -1;
        //        response.ResponseCode = "Not Logged In";
        //        return Json(response);
        //    }

        //    UserInfo user = UserDAL.GetUserByUserCode(lgUser);

        //    if (user == null)
        //    {
        //        response.ResponseStatus = -1;
        //        response.ResponseCode = "Invalid User";
        //        return Json(response);
        //    }

        //    if (user.PermissionGroup[PERM_ADD] != '1')
        //    {
        //        response.ResponseStatus = -2;
        //        response.ResponseCode = "Not Authorized";
        //        return Json(response);
        //    }

        //    if (string.IsNullOrWhiteSpace(Requisition.RequisitionName))
        //        response.ResponseMessages.Add("Requisition Name is required");

        //    if (string.IsNullOrWhiteSpace(Requisition.DepartmentId))
        //        response.ResponseMessages.Add("Department is required");

        //    if (string.IsNullOrWhiteSpace(Requisition.RequestedBy))
        //        response.ResponseMessages.Add("Requested By is required");

        //    var serializer = new JavaScriptSerializer();

        //    List<InvDepartmentRequisitionDetailInfo> details =
        //        string.IsNullOrWhiteSpace(DetailsJson)
        //            ? new List<InvDepartmentRequisitionDetailInfo>()
        //            : serializer.Deserialize<List<InvDepartmentRequisitionDetailInfo>>(DetailsJson);

        //    if (details == null || details.Count == 0)
        //        response.ResponseMessages.Add("At least one item must be added");

        //    if (response.ResponseMessages.Count > 0)
        //    {
        //        response.ResponseStatus = 0;
        //        response.ResponseCode = "Invalid Data";
        //        response.ResponseMessage =
        //            "<ul><li>" +
        //            response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j)
        //            + "</li></ul>";

        //        return Json(response);
        //    }

        //    try
        //    {
        //        // Calculate line totals
        //        foreach (var d in details)
        //        {
        //            d.TotalPriceEst = d.Quantity * d.UnitPriceEst;
        //        }

        //        Requisition.RowId = Guid.NewGuid().ToString();
        //        Requisition.Details = details;

        //        InvDepartmentRequisitionNewDAL.AddRequisition(Requisition, lgUser);
        //    }
        //    catch (Exception ex)
        //    {
        //        response.ResponseStatus = 0;
        //        response.ResponseCode = "Database Error";
        //        response.ResponseMessage = ex.Message;

        //        return Json(response);
        //    }

        //    response.ResponseStatus = 1;
        //    response.ResponseCode = "Success";
        //    response.ResponseMessage = "Requisition added successfully";

        //    return Json(response);
        //}
        [HttpPost]
        public ActionResult Add(
    InvDepartmentRequisitionInfo Requisition,
    string DetailsJson)
        {
            var response =
                new InvDepartmentRequisitionInfo.InvDepartmentRequisitionResponse();

            string lgUser = Session["LoggedInUser"] == null
                ? ""
                : Session["LoggedInUser"].ToString();

            if (string.IsNullOrWhiteSpace(lgUser))
            {
                response.ResponseStatus = -1;
                response.ResponseCode = "Not Logged In";
                response.ResponseMessage = "Please log in again.";
                return Json(response);
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
            {
                response.ResponseStatus = -1;
                response.ResponseCode = "Invalid User";
                response.ResponseMessage = "The logged-in user could not be verified.";
                return Json(response);
            }

            if (user.PermissionGroup[PERM_ADD] != '1')
            {
                response.ResponseStatus = -2;
                response.ResponseCode = "Not Authorized";
                response.ResponseMessage = "You are not authorized to add requisitions.";
                return Json(response);
            }

            try
            {
                if (Requisition == null)
                    throw new Exception("Requisition information is required.");

                if (string.IsNullOrWhiteSpace(Requisition.RequisitionName))
                    throw new Exception("Requisition Name is required.");

                if (string.IsNullOrWhiteSpace(Requisition.DepartmentId))
                    throw new Exception("Department is required.");

                if (string.IsNullOrWhiteSpace(Requisition.RequestedBy))
                    throw new Exception("Requested By is required.");

                List<InvDepartmentRequisitionDetailInfo> details;

                if (string.IsNullOrWhiteSpace(DetailsJson))
                {
                    details = new List<InvDepartmentRequisitionDetailInfo>();
                }
                else
                {
                    var serializer = new JavaScriptSerializer();
                    details =
                        serializer.Deserialize<List<InvDepartmentRequisitionDetailInfo>>(
                            DetailsJson);
                }

                if (details == null || details.Count == 0)
                    throw new Exception("At least one item must be added.");

                // Validate item lines before calling the DAL.
                foreach (var line in details)
                {
                    if (line == null)
                        throw new Exception("An invalid item line was submitted.");

                    if (string.IsNullOrWhiteSpace(line.ItemId))
                        throw new Exception("Please select an item.");

                    if (string.IsNullOrWhiteSpace(line.UnitId))
                        throw new Exception("Please select a unit.");

                    if (line.Quantity <= 0)
                        throw new Exception("Quantity must be greater than zero.");

                    if (line.UnitPriceEst < 0)
                        throw new Exception("Estimated unit price cannot be negative.");

                    // Do not rely on a client-supplied total.
                    line.TotalPriceEst = decimal.Round(
                        line.Quantity * line.UnitPriceEst,
                        2,
                        MidpointRounding.AwayFromZero);
                }

                Requisition.RowId = Guid.NewGuid().ToString();
                Requisition.Details = details;

                // IMPORTANT: use the second DAL, which writes all four tables.
                InvDepartmentRequisition2ndDAL.AddRequisition(
                    Requisition,
                    lgUser);

                response.ResponseStatus = 1;
                response.ResponseCode = "Success";
                response.ResponseMessage =
                    "Requisition and initial transaction added successfully.";
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Add Failed";
                response.ResponseMessage = ex.Message;
            }

            return Json(response);
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
                return Redirect(Url.Action("Logout", "Login"));

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Redirect(Url.Action("Logout", "Login"));

            if (user.PermissionGroup[PERM_EDIT] != '1')
                return Redirect(Url.Action("Index", "InvDepartmentRequisitionNew"));

            if (RouteData.Values["id"] == null)
                return Redirect(Url.Action("Index", "InvDepartmentRequisitionNew"));

            InvDepartmentRequisitionInfo requisition =
                InvDepartmentRequisition2ndDAL.GetRequisitionByID(RouteData.Values["id"].ToString());

            if (requisition == null)
                return Redirect(Url.Action("Index", "InvDepartmentRequisitionNew"));

            ViewBag.User = user;
            ViewBag.Requisition = requisition;

            ViewBag.DepartmentList = HrDepartmentDAL.GetAllHrDepartments();
            ViewBag.EmployeeList = HrEmployeeDAL.GetAllHrEmployees();
            ViewBag.JobList = InvJobDAL.GetAllInvJobs();
            ViewBag.ItemList = InvItemDAL.GetAllInvItems();
            ViewBag.UnitList = InvUnitDAL.GetAllInvUnits();

            return View();
        }

        [HttpPost]
        public ActionResult Edit(InvDepartmentRequisitionInfo Requisition, string DetailsJson)
        {
            InvDepartmentRequisitionInfo.InvDepartmentRequisitionResponse response =
                new InvDepartmentRequisitionInfo.InvDepartmentRequisitionResponse();

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

            if (user.PermissionGroup[PERM_EDIT] != '1')
            {
                response.ResponseStatus = -2;
                return Json(response);
            }

            if (string.IsNullOrWhiteSpace(Requisition.RowId))
                response.ResponseMessages.Add("Requisition Id is missing");

            if (string.IsNullOrWhiteSpace(Requisition.RequisitionName))
                response.ResponseMessages.Add("Requisition Name is required");

            if (string.IsNullOrWhiteSpace(Requisition.DepartmentId))
                response.ResponseMessages.Add("Department is required");

            if (string.IsNullOrWhiteSpace(Requisition.RequestedBy))
                response.ResponseMessages.Add("Requested By is required");

            var serializer = new JavaScriptSerializer();

            List<InvDepartmentRequisitionDetailInfo> details =
                string.IsNullOrWhiteSpace(DetailsJson)
                    ? new List<InvDepartmentRequisitionDetailInfo>()
                    : serializer.Deserialize<List<InvDepartmentRequisitionDetailInfo>>(DetailsJson);

            if (details == null || details.Count == 0)
                response.ResponseMessages.Add("At least one item must be added");

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
                // Calculate line totals
                foreach (var d in details)
                {
                    d.TotalPriceEst = d.Quantity * d.UnitPriceEst;
                }

                Requisition.Details = details;

                InvDepartmentRequisition2ndDAL.UpdateRequisition(Requisition, lgUser);
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
            response.ResponseMessage = "Requisition updated successfully";

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
                return Redirect(Url.Action("Logout", "Login"));

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Redirect(Url.Action("Logout", "Login"));

            if (user.PermissionGroup[PERM_DELETE] != '1')
                return Redirect(Url.Action("Index", "InvDepartmentRequisitionNew"));

            if (RouteData.Values["id"] == null)
                return Redirect(Url.Action("Index", "InvDepartmentRequisitionNew"));

            InvDepartmentRequisitionInfo requisition =
                InvDepartmentRequisition2ndDAL.GetRequisitionByID(RouteData.Values["id"].ToString());

            if (requisition == null)
                return Redirect(Url.Action("Index", "InvDepartmentRequisitionNew"));

            ViewBag.User = user;
            ViewBag.Requisition = requisition;

            return View();
        }

       
[HttpPost]
public ActionResult Delete(InvDepartmentRequisitionInfo Requisition)
        {
            InvDepartmentRequisitionInfo.InvDepartmentRequisitionResponse response =
                new InvDepartmentRequisitionInfo.InvDepartmentRequisitionResponse();

            string lgUser = "";

            // ---------------------------------------------------------
            // 1. Get logged-in user
            // ---------------------------------------------------------

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                response.ResponseCode = "Session Expired";
                response.ResponseMessage = "Logged in user was not found.";

                return Json(response);
            }


            // ---------------------------------------------------------
            // 2. Get user information
            // ---------------------------------------------------------

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
            {
                response.ResponseStatus = -1;
                response.ResponseCode = "User Not Found";
                response.ResponseMessage = "User information was not found.";

                return Json(response);
            }


            // ---------------------------------------------------------
            // 3. Check delete permission
            // ---------------------------------------------------------

            if (user.PermissionGroup[PERM_DELETE] != '1')
            {
                response.ResponseStatus = -2;
                response.ResponseCode = "Permission Denied";
                response.ResponseMessage =
                    "You do not have permission to delete requisition.";

                return Json(response);
            }


            // ---------------------------------------------------------
            // 4. Validate requisition
            // ---------------------------------------------------------

            if (Requisition == null ||
                string.IsNullOrWhiteSpace(Requisition.RowId))
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Request";
                response.ResponseMessage =
                    "Requisition RowId is required.";

                return Json(response);
            }


            // ---------------------------------------------------------
            // 5. Delete requisition
            // ---------------------------------------------------------

            try
            {
                InvDepartmentRequisition2ndDAL.DeleteRequisition(
                    Requisition.RowId,lgUser);

                response.ResponseStatus = 1;
                response.ResponseCode = "Success";
                response.ResponseMessage =
                    "Requisition deleted successfully.";
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;
            }

            return Json(response);
        }



        #endregion

        #region Workflow Actions

        [HttpPost]
        public ActionResult Forward(string id, string remarks = null)
        {
            return RunWorkflow(id, remarks, PERM_FORWARD, (rid, usr, rem) =>
                InvDepartmentRequisition2ndDAL.ForwardRequisition(rid, usr, rem),
                "Requisition forwarded successfully");
        }

        [HttpPost]
        public ActionResult Approve(string id, string remarks = null)
        {
            return RunWorkflow(id, remarks, PERM_APPROVE, (rid, usr, rem) =>
                InvDepartmentRequisition2ndDAL.ApproveRequisition(rid, usr, rem),
                "Requisition approved successfully");
        }

        [HttpPost]
        public ActionResult Reject(string id, string remarks = null)
        {
            return RunWorkflow(id, remarks, PERM_REJECT, (rid, usr, rem) =>
                InvDepartmentRequisition2ndDAL.RejectRequisition(rid, usr, rem),
                "Requisition rejected successfully");
        }

        [HttpPost]
        public ActionResult Issue(string id, string remarks = null)
        {
            return RunWorkflow(id, remarks, PERM_ISSUE, (rid, usr, rem) =>
                InvDepartmentRequisition2ndDAL.IssueRequisition(rid, usr, rem),
                "Requisition issued successfully");
        }

        // Shared session / permission / try-catch handling for the workflow actions
        private ActionResult RunWorkflow(string id, string remarks, int permissionIndex,
            Action<string, string, string> dalAction, string successMessage)
        {
            InvDepartmentRequisitionInfo.InvDepartmentRequisitionResponse response =
                new InvDepartmentRequisitionInfo.InvDepartmentRequisitionResponse();

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

            if (user.PermissionGroup[permissionIndex] != '1')
            {
                response.ResponseStatus = -2;
                response.ResponseCode = "Not Authorized";
                return Json(response);
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";
                response.ResponseMessage = "Requisition Id is missing";
                return Json(response);
            }

            try
            {
                dalAction(id, lgUser, remarks);

                response.ResponseStatus = 1;
                response.ResponseCode = "Success";
                response.ResponseMessage = successMessage;
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;
            }

            return Json(response);
        }

        #endregion
    }
}
