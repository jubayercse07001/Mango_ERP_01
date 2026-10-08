using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ERP.DAL;
using ERP.DAL.Inventory;
using ERP.DAL.HR.Organization;
using ERP.DAL.Inventory.InvJob;
using ERP.DAL.HR.Employee;
using ERP.DAL.HR.Common;
using ERP.Models.User;
using ERP.Models.Inventory.InvDepartmentRequisition;
using ERP.Models.Inventory.InvDepartmentRequisitionDetail;

namespace ERP.Controllers.Inventory
{
    public class InvDepartmentRequisitionOperationController : Controller
    {
        private const int PERM_VIEW = 0;
        private const int PERM_ADD = 1;
        private const int PERM_EDIT = 2;
        private const int PERM_DELETE = 3;
        private const int PERM_FORWARD = PERM_EDIT;
        private const int PERM_APPROVE = PERM_EDIT;
        private const int PERM_REJECT = PERM_EDIT;
        private const string SELF = "InvDepartmentRequisitionOperation";

        #region Index / List

        public ActionResult Index()
        {
            string lgUser = Session["LoggedInUser"] as string;
            if (string.IsNullOrEmpty(lgUser))
                return Redirect(Url.Action("Logout", "Login"));

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null)
                return Redirect(Url.Action("Logout", "Login"));

            if (user.PermissionGroup[PERM_VIEW] != '1')
                return Redirect(Url.Action("Index", "Home"));

            ViewBag.User = user;
            ViewBag.DepartmentList = HrDepartmentDAL.GetAllHrDepartments();
            return View();
        }

        [HttpGet]
        public ActionResult GetInvDepartmentRequisitionItemList()
        {
            string lgUser = Session["LoggedInUser"] as string;
            if (string.IsNullOrEmpty(lgUser))
            {
                return Json(new
                {
                    data = new List<InvDepartmentRequisitionInfo>(),
                    draw = Request["draw"],
                    recordsTotal = 0,
                    recordsFiltered = 0
                }, JsonRequestBehavior.AllowGet);
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null)
            {
                return Json(new
                {
                    data = new List<InvDepartmentRequisitionInfo>(),
                    draw = Request["draw"],
                    recordsTotal = 0,
                    recordsFiltered = 0
                }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var resp = InvDepartmentRequisitionOperationDAL.GetFilteredRequisitions(Request, lgUser);
                return Json(new
                {
                    data = resp.Requisitions,
                    draw = Request["draw"],
                    recordsTotal = resp.TotalRecordCount,
                    recordsFiltered = resp.FilteredRecordCount
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    data = new List<InvDepartmentRequisitionInfo>(),
                    draw = Request["draw"],
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Add

        [HttpGet]
        public ActionResult Add()
        {
            string lgUser = Session["LoggedInUser"] as string;
            if (string.IsNullOrEmpty(lgUser))
                return Redirect(Url.Action("Logout", "Login"));

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null)
                return Redirect(Url.Action("Logout", "Login"));

            if (user.PermissionGroup[PERM_ADD] != '1')
                return Redirect(Url.Action("Index", SELF));

            ViewBag.User = user;
            ViewBag.DepartmentList = HrDepartmentDAL.GetAllHrDepartments();
            ViewBag.EmployeeList = HrEmployeeDAL.GetAllHrEmployees();
            ViewBag.JobList = InvJobDAL.GetAllInvJobs();
            ViewBag.ItemList = InvItemDAL.GetAllInvItems();
            ViewBag.UnitList = InvUnitDAL.GetAllInvUnits();
            ViewBag.NextRequisitionCode = InvDepartmentRequisitionOperationDAL.GetNextRequisitionCode();
            return View();
        }

        [HttpPost]
        public ActionResult Add(InvDepartmentRequisitionInfo Requisition, string DetailsJson)
        {
            var response = new InvDepartmentRequisitionInfo.InvDepartmentRequisitionResponse();
            string lgUser = Session["LoggedInUser"] as string;

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

            if (user.PermissionGroup[PERM_ADD] != '1')
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

            if (details == null || details.Count == 0)
                response.ResponseMessages.Add("At least one item must be added");

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";
                response.ResponseMessage =
                    "<ul><li>" +
                    string.Join("</li><li>", response.ResponseMessages) +
                    "</li></ul>";
                return Json(response);
            }

            try
            {
                foreach (var d in details)
                    d.TotalPriceEst = d.Quantity * d.UnitPriceEst;

                Requisition.RowId = Guid.NewGuid().ToString();
                Requisition.Details = details;
                InvDepartmentRequisitionOperationDAL.AddRequisition(Requisition, lgUser);

                response.ResponseStatus = 1;
                response.ResponseCode = "Success";
                response.ResponseMessage = "Requisition added successfully";
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.GetBaseException().Message;
            }

            return Json(response);
        }

        #endregion

        #region Edit

        [HttpGet]
        public ActionResult Edit()
        {
            string lgUser = Session["LoggedInUser"] as string;
            if (string.IsNullOrEmpty(lgUser))
                return Redirect(Url.Action("Logout", "Login"));

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null)
                return Redirect(Url.Action("Logout", "Login"));

            if (user.PermissionGroup[PERM_EDIT] != '1')
                return Redirect(Url.Action("Index", SELF));

            if (RouteData.Values["id"] == null)
                return Redirect(Url.Action("Index", SELF));

            var requisition = InvDepartmentRequisitionOperationDAL
                .GetRequisitionByID(RouteData.Values["id"].ToString());

            if (requisition == null)
                return Redirect(Url.Action("Index", SELF));

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
            var response = new InvDepartmentRequisitionInfo.InvDepartmentRequisitionResponse();
            string lgUser = Session["LoggedInUser"] as string;

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
                    string.Join("</li><li>", response.ResponseMessages) +
                    "</li></ul>";
                return Json(response);
            }

            try
            {
                foreach (var d in details)
                    d.TotalPriceEst = d.Quantity * d.UnitPriceEst;

                Requisition.Details = details;
                InvDepartmentRequisitionOperationDAL.UpdateRequisition(Requisition, lgUser);

                response.ResponseStatus = 1;
                response.ResponseCode = "Success";
                response.ResponseMessage = "Requisition updated successfully";
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.GetBaseException().Message;
            }

            return Json(response);
        }

        #endregion

        #region Delete

        [HttpPost]
        public ActionResult Delete(InvDepartmentRequisitionInfo Requisition)
        {
            var response = new InvDepartmentRequisitionInfo.InvDepartmentRequisitionResponse();
            string lgUser = Session["LoggedInUser"] as string;

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

            if (user.PermissionGroup[PERM_DELETE] != '1')
            {
                response.ResponseStatus = -2;
                return Json(response);
            }

            try
            {
                InvDepartmentRequisitionOperationDAL.DeleteRequisition(Requisition, lgUser);
                response.ResponseStatus = 1;
                response.ResponseCode = "Success";
                response.ResponseMessage = "Requisition deleted successfully";
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.GetBaseException().Message;
            }

            return Json(response);
        }

        #endregion

        #region Forward

        [HttpGet]
        public ActionResult Forward(string id)
        {
            try
            {
                if (Session["LoggedInUser"] == null)
                    return RedirectToAction("Logout", "Login");

                string loggedInUser = Session["LoggedInUser"].ToString();
                UserInfo user = UserDAL.GetUserByUserCode(loggedInUser);

                if (user == null)
                    return RedirectToAction("Logout", "Login");

                if (user.PermissionGroup[PERM_FORWARD] != '1')
                {
                    return new HttpStatusCodeResult(
                        System.Net.HttpStatusCode.Forbidden,
                        "You are not authorized to forward requisition.");
                }

                if (string.IsNullOrWhiteSpace(id))
                    return RedirectToAction("Index");

                var requisition = InvDepartmentRequisitionOperationDAL.GetRequisitionByID(id);
                if (requisition == null)
                {
                    TempData["ErrorMessage"] = "Requisition was not found.";
                    return RedirectToAction("Index");
                }

                if (requisition.Status != 0)
                {
                    TempData["ErrorMessage"] = "Only New requisition can be forwarded.";
                    return RedirectToAction("Index");
                }

                ViewBag.DepartmentList = HrDepartmentDAL.GetAllHrDepartments();
                ViewBag.Requisition = requisition;
                ViewBag.User = user;
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Unable to load Forward page: " + ex.GetBaseException().Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public JsonResult Forward(string id, string toDepartmentId, string remarks)
        {
            try
            {
                if (Session["LoggedInUser"] == null)
                {
                    return Json(new
                    {
                        ResponseStatus = -1,
                        ResponseMessage = "Your session has expired. Please login again."
                    });
                }

                string loggedInUser = Session["LoggedInUser"].ToString();
                UserInfo user = UserDAL.GetUserByUserCode(loggedInUser);

                if (user == null)
                {
                    return Json(new
                    {
                        ResponseStatus = -1,
                        ResponseMessage = "Invalid user."
                    });
                }

                if (user.PermissionGroup[PERM_FORWARD] != '1')
                {
                    return Json(new
                    {
                        ResponseStatus = -2,
                        ResponseMessage = "You are not authorized to forward requisition."
                    });
                }

                if (string.IsNullOrWhiteSpace(id))
                {
                    return Json(new
                    {
                        ResponseStatus = 0,
                        ResponseMessage = "Requisition ID is required."
                    });
                }

                if (string.IsNullOrWhiteSpace(toDepartmentId))
                {
                    return Json(new
                    {
                        ResponseStatus = 0,
                        ResponseMessage = "Please select To Department."
                    });
                }

                // Get the original Department of the Requisition (this will be used as FromDepartment)
                var requisition = InvDepartmentRequisitionOperationDAL.GetRequisitionByID(id);
                if (requisition == null)
                {
                    return Json(new
                    {
                        ResponseStatus = 0,
                        ResponseMessage = "Requisition was not found."
                    });
                }

                string fromDepartmentId = requisition.DepartmentId;

                if (string.IsNullOrWhiteSpace(fromDepartmentId))
                {
                    return Json(new
                    {
                        ResponseStatus = 0,
                        ResponseMessage = "From Department is missing on the requisition."
                    });
                }

                string message;
                bool success = InvDepartmentRequisitionOperationDAL.ForwardRequisition(
                    id,                    // rowId
                    fromDepartmentId,      // fromDepartmentId  ← newly added
                    toDepartmentId,        // toDepartmentId
                    loggedInUser,
                    remarks,
                    out message);

                return Json(new
                {
                    ResponseStatus = success ? 1 : 0,
                    ResponseMessage = message
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    ResponseStatus = 0,
                    ResponseMessage = "Forward failed: " + ex.GetBaseException().Message
                });
            }
        }

        #endregion

        #region Approved

        [HttpGet]
        public ActionResult Approved()
        {
            string lgUser = Session["LoggedInUser"] as string;
            if (string.IsNullOrWhiteSpace(lgUser))
                return Redirect(Url.Action("Logout", "Login"));

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null)
                return Redirect(Url.Action("Logout", "Login"));

            if (user.PermissionGroup[PERM_APPROVE] != '1')
                return Redirect(Url.Action("Index", SELF));

            if (RouteData.Values["id"] == null)
                return Redirect(Url.Action("Index", SELF));

            string rowId = RouteData.Values["id"].ToString();
            var requisition = InvDepartmentRequisitionOperationDAL.GetRequisitionByID(rowId);

            if (requisition == null)
                return Redirect(Url.Action("Index", SELF));

            ViewBag.User = user;
            ViewBag.Requisition = requisition;
            ViewBag.DepartmentList = HrDepartmentDAL.GetAllHrDepartments();
            return View();
        }

        [HttpPost]
        public JsonResult Approved(string id, string toDepartmentId, string remarks, string ApprovedLinesJson)
        {
            try
            {
                string loggedInUser = Session["LoggedInUser"] as string;
                if (string.IsNullOrWhiteSpace(loggedInUser))
                {
                    return Json(new
                    {
                        ResponseStatus = -1,
                        ResponseMessage = "Your session has expired. Please login again."
                    });
                }

                UserInfo user = UserDAL.GetUserByUserCode(loggedInUser);
                if (user == null)
                {
                    return Json(new
                    {
                        ResponseStatus = -1,
                        ResponseMessage = "Invalid user."
                    });
                }

                if (user.PermissionGroup[PERM_APPROVE] != '1')
                {
                    return Json(new
                    {
                        ResponseStatus = -2,
                        ResponseMessage = "You are not authorized to approve requisition."
                    });
                }

                if (string.IsNullOrWhiteSpace(id))
                {
                    return Json(new
                    {
                        ResponseStatus = 0,
                        ResponseMessage = "Requisition ID is required."
                    });
                }

                if (string.IsNullOrWhiteSpace(toDepartmentId))
                {
                    return Json(new
                    {
                        ResponseStatus = 0,
                        ResponseMessage = "Please select To Department."
                    });
                }

                if (string.IsNullOrWhiteSpace(ApprovedLinesJson))
                {
                    return Json(new
                    {
                        ResponseStatus = 0,
                        ResponseMessage = "Approval details are required."
                    });
                }

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                List<InvDepartmentRequisitionApprovedLine> approvedLines;

                try
                {
                    approvedLines = serializer.Deserialize<List<InvDepartmentRequisitionApprovedLine>>(ApprovedLinesJson);
                }
                catch
                {
                    return Json(new
                    {
                        ResponseStatus = 0,
                        ResponseMessage = "Invalid approval details."
                    });
                }

                if (approvedLines == null || approvedLines.Count == 0)
                {
                    return Json(new
                    {
                        ResponseStatus = 0,
                        ResponseMessage = "No approval items were submitted."
                    });
                }

                foreach (var line in approvedLines)
                {
                    if (line == null || string.IsNullOrWhiteSpace(line.RowId))
                    {
                        return Json(new
                        {
                            ResponseStatus = 0,
                            ResponseMessage = "Invalid requisition detail RowId."
                        });
                    }

                    if (line.ApprovedQuantity < 0)
                    {
                        return Json(new
                        {
                            ResponseStatus = 0,
                            ResponseMessage = "Approved quantity cannot be negative."
                        });
                    }
                }

                string message;
                bool success = InvDepartmentRequisitionOperationDAL.ApproveRequisition(
                    id, toDepartmentId, remarks, approvedLines, loggedInUser, out message);

                return Json(new
                {
                    ResponseStatus = success ? 1 : 0,
                    ResponseMessage = message
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    ResponseStatus = 0,
                    ResponseMessage = "Approval failed: " + ex.GetBaseException().Message
                });
            }
        }

        #endregion

        #region Reject

        [HttpGet]
        public ActionResult Reject()
        {
            string loggedInUser = Session["LoggedInUser"] as string;
            if (string.IsNullOrWhiteSpace(loggedInUser))
                return Redirect(Url.Action("Logout", "Login"));

            UserInfo user = UserDAL.GetUserByUserCode(loggedInUser);
            if (user == null)
                return Redirect(Url.Action("Logout", "Login"));

            if (user.PermissionGroup[PERM_REJECT] != '1')
                return Redirect(Url.Action("Index", SELF));

            if (RouteData.Values["id"] == null)
                return Redirect(Url.Action("Index", SELF));

            string rowId = RouteData.Values["id"].ToString();
            var requisition = InvDepartmentRequisitionOperationDAL.GetRequisitionByID(rowId);

            if (requisition == null)
                return Redirect(Url.Action("Index", SELF));

            ViewBag.User = user;
            ViewBag.Requisition = requisition;
            return View();
        }

        [HttpPost]
        public JsonResult Reject(string id, string remarks)
        {
            try
            {
                string loggedInUser = Session["LoggedInUser"] as string;
                if (string.IsNullOrWhiteSpace(loggedInUser))
                {
                    return Json(new
                    {
                        ResponseStatus = -1,
                        ResponseMessage = "Your session has expired. Please login again."
                    });
                }

                UserInfo user = UserDAL.GetUserByUserCode(loggedInUser);
                if (user == null)
                {
                    return Json(new
                    {
                        ResponseStatus = -1,
                        ResponseMessage = "Invalid user."
                    });
                }

                if (user.PermissionGroup[PERM_REJECT] != '1')
                {
                    return Json(new
                    {
                        ResponseStatus = -2,
                        ResponseMessage = "You are not authorized to reject requisition."
                    });
                }

                if (string.IsNullOrWhiteSpace(id))
                {
                    return Json(new
                    {
                        ResponseStatus = 0,
                        ResponseMessage = "Requisition ID is required."
                    });
                }

                if (string.IsNullOrWhiteSpace(remarks))
                {
                    return Json(new
                    {
                        ResponseStatus = 0,
                        ResponseMessage = "Rejection reason is required."
                    });
                }

                string message;
                bool success = InvDepartmentRequisitionOperationDAL.RejectRequisition(
                    id, loggedInUser, remarks, out message);

                return Json(new
                {
                    ResponseStatus = success ? 1 : 0,
                    ResponseMessage = message
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    ResponseStatus = 0,
                    ResponseMessage = "Rejection failed: " + ex.GetBaseException().Message
                });
            }
        }

        #endregion
    }
}