using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Script.Serialization;

using ERP.DAL;
using ERP.DAL.Inventory;
using ERP.DAL.HR.Organization;
using ERP.Models.User;
using ERP.Models.Inventory.InvDepartmentRequisition;
using ERP.DAL.HR.Common;

namespace ERP.Controllers.Inventory
{
    public class InvDepartmentTransactionReceiveController : Controller
    {
        private const int PERM_VIEW = 0;
        private const int PERM_EDIT = 2;
        private const int STATUS_ISSUED = 4;
        private const int STATUS_RECEIVED = 5;

        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                string loggedInUser = Session["LoggedInUser"] as string;
                if (string.IsNullOrWhiteSpace(loggedInUser))
                    return RedirectToAction("Logout", "Login");

                UserInfo user = UserDAL.GetUserByUserCode(loggedInUser);
                if (user == null)
                    return RedirectToAction("Logout", "Login");

                if (user.PermissionGroup == null ||
                    user.PermissionGroup.Length <= PERM_VIEW ||
                    user.PermissionGroup[PERM_VIEW] != '1')
                    return RedirectToAction("Index", "Home");

                ViewBag.User = user;
                ViewBag.DepartmentList = HrDepartmentDAL.GetAllHrDepartments();
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    "Unable to load Department Transaction Receive: " +
                    ex.GetBaseException().Message;
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public JsonResult GetInvDepartmentRequisitionItemList()
        {
            string loggedInUser = Session["LoggedInUser"] as string;
            if (string.IsNullOrWhiteSpace(loggedInUser))
            {
                return Json(new
                {
                    data = new List<InvDepartmentRequisitionInfo>(),
                    draw = Request["draw"],
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    error = "Your session has expired."
                }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                UserInfo user = UserDAL.GetUserByUserCode(loggedInUser);
                if (user == null ||
                    user.PermissionGroup == null ||
                    user.PermissionGroup.Length <= PERM_VIEW ||
                    user.PermissionGroup[PERM_VIEW] != '1')
                {
                    return Json(new
                    {
                        data = new List<InvDepartmentRequisitionInfo>(),
                        draw = Request["draw"],
                        recordsTotal = 0,
                        recordsFiltered = 0,
                        error = "You are not authorized."
                    }, JsonRequestBehavior.AllowGet);
                }

                var response = InvDepartmentTransactionReceiveDAL
                    .GetFilteredIssuedRequisitions(Request, loggedInUser);

                return Json(new
                {
                    data = response.Requisitions,
                    draw = Request["draw"],
                    recordsTotal = response.TotalRecordCount,
                    recordsFiltered = response.FilteredRecordCount
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
                    error = ex.GetBaseException().Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult Receive(string id)
        {
            try
            {
                string loggedInUser = Session["LoggedInUser"] as string;
                if (string.IsNullOrWhiteSpace(loggedInUser))
                    return RedirectToAction("Logout", "Login");

                UserInfo user = UserDAL.GetUserByUserCode(loggedInUser);
                if (user == null)
                    return RedirectToAction("Logout", "Login");

                if (user.PermissionGroup == null ||
                    user.PermissionGroup.Length <= PERM_EDIT ||
                    user.PermissionGroup[PERM_EDIT] != '1')
                {
                    TempData["ErrorMessage"] = "You are not authorized to receive requisition.";
                    return RedirectToAction("Index");
                }

                if (string.IsNullOrWhiteSpace(id))
                    id = Request.QueryString["id"];
                if (string.IsNullOrWhiteSpace(id) && RouteData.Values["id"] != null)
                    id = Convert.ToString(RouteData.Values["id"]);

                id = Convert.ToString(id).Trim();
                if (string.IsNullOrWhiteSpace(id))
                {
                    TempData["ErrorMessage"] = "Requisition RowId is missing.";
                    return RedirectToAction("Index");
                }

                var requisition = InvDepartmentTransactionReceiveDAL.GetRequisitionByID(id);
                if (requisition == null)
                {
                    TempData["ErrorMessage"] =
                        "Issued requisition was not found. RowId: " + id;
                    return RedirectToAction("Index");
                }

                int status = Convert.ToInt32(requisition.Status);
                int isIssued = Convert.ToInt32(requisition.IsIssued);
                int isReceived = 0;
                try { isReceived = Convert.ToInt32(requisition.IsReceived); } catch { }

                if (status != STATUS_ISSUED || isIssued != 1 || isReceived == 1)
                {
                    TempData["ErrorMessage"] =
                        "Only Issued and not-yet-received requisitions can be received. " +
                        "Status=" + status + ", IsIssued=" + isIssued + ", IsReceived=" + isReceived;
                    return RedirectToAction("Index");
                }

                var receivedQuantityMap = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                if (requisition.Details != null)
                {
                    foreach (var d in requisition.Details)
                    {
                        string key = Convert.ToString(d.RowId);
                        if (!string.IsNullOrWhiteSpace(key))
                            receivedQuantityMap[key] = d.ReceivedQuantity;
                    }
                }

                ViewBag.User = user;
                ViewBag.Requisition = requisition;
                ViewBag.ReceivedQuantityMap = receivedQuantityMap;
                ViewBag.DepartmentList = HrDepartmentDAL.GetAllHrDepartments();
                ViewBag.UnitList = InvUnitDAL.GetAllInvUnits();
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    "Unable to load Receive page: " + ex.GetBaseException().Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public JsonResult Receive(string id, string remarks, string ReceivedLinesJson)
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

                if (user.PermissionGroup == null ||
                    user.PermissionGroup.Length <= PERM_EDIT ||
                    user.PermissionGroup[PERM_EDIT] != '1')
                {
                    return Json(new
                    {
                        ResponseStatus = -2,
                        ResponseMessage = "You are not authorized to receive requisition."
                    });
                }

                if (string.IsNullOrWhiteSpace(id))
                {
                    return Json(new
                    {
                        ResponseStatus = 0,
                        ResponseMessage = "Requisition RowId is required."
                    });
                }

                if (string.IsNullOrWhiteSpace(ReceivedLinesJson))
                {
                    return Json(new
                    {
                        ResponseStatus = 0,
                        ResponseMessage = "Receive details are required."
                    });
                }

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                List<InvDepartmentRequisitionReceivedLine> receivedLines;
                try
                {
                    receivedLines = serializer
                        .Deserialize<List<InvDepartmentRequisitionReceivedLine>>(ReceivedLinesJson);
                }
                catch
                {
                    return Json(new
                    {
                        ResponseStatus = 0,
                        ResponseMessage = "Invalid receive details."
                    });
                }

                if (receivedLines == null || receivedLines.Count == 0)
                {
                    return Json(new
                    {
                        ResponseStatus = 0,
                        ResponseMessage = "No receive items were submitted."
                    });
                }

                foreach (var line in receivedLines)
                {
                    if (line == null || string.IsNullOrWhiteSpace(line.RowId))
                    {
                        return Json(new
                        {
                            ResponseStatus = 0,
                            ResponseMessage = "Invalid requisition detail RowId."
                        });
                    }
                    if (line.ReceivedQuantity < 0)
                    {
                        return Json(new
                        {
                            ResponseStatus = 0,
                            ResponseMessage = "Received quantity cannot be negative."
                        });
                    }
                }

                string message;
                bool success = InvDepartmentTransactionReceiveDAL.ReceiveRequisition(
                    id.Trim(),
                    remarks,
                    receivedLines,
                    loggedInUser,
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
                    ResponseMessage = "Receive failed: " + ex.GetBaseException().Message
                });
            }

        }
        [HttpGet]
        public JsonResult GetUnitConversion(
    string requestedUnitId,
    string issueUnitId,
    decimal unitPriceEst)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(requestedUnitId) ||
                    string.IsNullOrWhiteSpace(issueUnitId))
                {
                    return Json(new { success = false, message = "Unit is required." },
                        JsonRequestBehavior.AllowGet);
                }

                if (string.Equals(requestedUnitId, issueUnitId, StringComparison.OrdinalIgnoreCase))
                {
                    decimal price = Math.Round(unitPriceEst, 2, MidpointRounding.AwayFromZero);
                    return Json(new
                    {
                        success = true,
                        sameUnit = true,
                        conversionRate = 1m,
                        conversionPrice = price,
                        unitConversionId = (string)null
                    }, JsonRequestBehavior.AllowGet);
                }

                var conversion = InvDepartmentTransactionReceiveDAL
                    .GetUnitConversionPublic(requestedUnitId, issueUnitId);

                if (conversion == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No unit conversion exists between the selected units."
                    }, JsonRequestBehavior.AllowGet);
                }

                if (conversion.ConversionRate <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid conversion rate."
                    }, JsonRequestBehavior.AllowGet);
                }

                decimal rate;
                if (string.Equals(conversion.FirstUnitId, requestedUnitId, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(conversion.SecondUnitId, issueUnitId, StringComparison.OrdinalIgnoreCase))
                {
                    rate = conversion.ConversionRate;
                }
                else
                {
                    rate = 1m / conversion.ConversionRate;
                }

                if (rate <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid calculated conversion rate."
                    }, JsonRequestBehavior.AllowGet);
                }

                decimal conversionPrice = Math.Round(unitPriceEst / rate, 2, MidpointRounding.AwayFromZero);

                return Json(new
                {
                    success = true,
                    sameUnit = false,
                    conversionRate = Math.Round(rate, 6, MidpointRounding.AwayFromZero),
                    conversionPrice = conversionPrice,
                    unitConversionId = conversion.RowId
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.GetBaseException().Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}