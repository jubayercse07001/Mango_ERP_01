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
    public class InvDepartmentRequisitionController : Controller
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

            ViewBag.User = user;

            // dropdowns for the Advance Search panel
            ViewBag.DepartmentList = HrDepartmentDAL.GetAllHrDepartments();

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
                return Redirect(Url.Action("Index", "InvDepartmentRequisition"));

            ViewBag.User = user;

            // dropdowns
            ViewBag.DepartmentList = HrDepartmentDAL.GetAllHrDepartments();
            ViewBag.EmployeeList = HrEmployeeDAL.GetAllHrEmployees();
            ViewBag.JobList = InvJobDAL.GetAllInvJobs();
            ViewBag.ItemList = InvItemDAL.GetAllInvItems();
            ViewBag.UnitList = InvUnitDAL.GetAllInvUnits();

            ViewBag.NextRequisitionCode = InvDepartmentRequisitionDAL.GetNextRequisitionCode();

            return View();
        }

        [HttpPost]
        public ActionResult Add(InvDepartmentRequisitionInfo Requisition, string DetailsJson)
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

            if (user.PermissionGroup[1] != '1')
            {
                response.ResponseStatus = -2;
                response.ResponseCode = "Not Authorized";
                return Json(response);
            }

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

            if (details.Count == 0)
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
                Requisition.RowId = Guid.NewGuid().ToString();
                Requisition.Details = details;

                InvDepartmentRequisitionDAL.AddRequisition(Requisition, lgUser);
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
            response.ResponseMessage = "Requisition added successfully";

            return Json(response);
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

            InvDepartmentRequisitionDAL.InvDepartmentRequisitionResponse resp;

            try
            {
                resp = InvDepartmentRequisitionDAL.GetFilteredRequisitions(Request);
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
                return Redirect(Url.Action("Index", "InvDepartmentRequisition"));

            InvDepartmentRequisitionInfo requisition =
                InvDepartmentRequisitionDAL.GetRequisitionByID(RouteData.Values["id"].ToString());

            if (requisition == null)
                return Redirect(Url.Action("Index", "InvDepartmentRequisition"));

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

            if (user.PermissionGroup[2] != '1')
            {
                response.ResponseStatus = -2;
                return Json(response);
            }

            if (string.IsNullOrWhiteSpace(Requisition.RequisitionName))
                response.ResponseMessages.Add("Requisition Name is required");

            if (string.IsNullOrWhiteSpace(Requisition.DepartmentId))
                response.ResponseMessages.Add("Department is required");

            var serializer = new JavaScriptSerializer();

            List<InvDepartmentRequisitionDetailInfo> details =
                string.IsNullOrWhiteSpace(DetailsJson)
                    ? new List<InvDepartmentRequisitionDetailInfo>()
                    : serializer.Deserialize<List<InvDepartmentRequisitionDetailInfo>>(DetailsJson);

            if (details.Count == 0)
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
                Requisition.Details = details;

                InvDepartmentRequisitionDAL.UpdateRequisition(Requisition, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;

                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "Requisition updated successfully";

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
                return Redirect(Url.Action("Index", "InvDepartmentRequisition"));

            InvDepartmentRequisitionInfo requisition =
                InvDepartmentRequisitionDAL.GetRequisitionByID(RouteData.Values["id"].ToString());

            if (requisition == null)
                return Redirect(Url.Action("Index", "InvDepartmentRequisition"));

            ViewBag.User = user;
            ViewBag.Requisition = requisition;

            return View();
        }

        [HttpPost]
        public ActionResult Delete(InvDepartmentRequisitionInfo Requisition)
        {
            InvDepartmentRequisitionInfo.InvDepartmentRequisitionResponse response =
                new InvDepartmentRequisitionInfo.InvDepartmentRequisitionResponse();

            string lgUser = Session["LoggedInUser"]?.ToString();

            try
            {
                InvDepartmentRequisitionDAL.DeleteRequisition(Requisition, lgUser);

                response.ResponseStatus = 1;
                response.ResponseMessage = "Requisition deleted successfully";
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
