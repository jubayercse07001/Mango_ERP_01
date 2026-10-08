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
    public class InvDepartmentTransactionIssueController : Controller
    {
        // =========================================================
        // PERMISSION
        // =========================================================

        private const int PERM_VIEW = 0;
        private const int PERM_EDIT = 2;


        // =========================================================
        // INDEX
        // =========================================================

        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                string loggedInUser =
                    Session["LoggedInUser"] as string;

                if (string.IsNullOrWhiteSpace(loggedInUser))
                {
                    return RedirectToAction(
                        "Logout",
                        "Login");
                }


                UserInfo user =
                    UserDAL.GetUserByUserCode(loggedInUser);

                if (user == null)
                {
                    return RedirectToAction(
                        "Logout",
                        "Login");
                }


                if (user.PermissionGroup[PERM_VIEW] != '1')
                {
                    return RedirectToAction(
                        "Index",
                        "Home");
                }


                ViewBag.User = user;

                ViewBag.DepartmentList =
                    HrDepartmentDAL.GetAllHrDepartments();


                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    "Unable to load Department Transaction Issue: " +
                    ex.GetBaseException().Message;

                return RedirectToAction(
                    "Index",
                    "Home");
            }
        }


        // =========================================================
        // DATATABLE
        // ONLY APPROVED + NOT ISSUED
        // =========================================================

        [HttpGet]
        public JsonResult GetInvDepartmentRequisitionItemList()
        {
            string loggedInUser =
                Session["LoggedInUser"] as string;


            if (string.IsNullOrWhiteSpace(loggedInUser))
            {
                return Json(
                    new
                    {
                        data =
                            new List<InvDepartmentRequisitionInfo>(),

                        draw = Request["draw"],

                        recordsTotal = 0,

                        recordsFiltered = 0,

                        error =
                            "Your session has expired."
                    },
                    JsonRequestBehavior.AllowGet);
            }


            UserInfo user =
                UserDAL.GetUserByUserCode(loggedInUser);


            if (user == null)
            {
                return Json(
                    new
                    {
                        data =
                            new List<InvDepartmentRequisitionInfo>(),

                        draw = Request["draw"],

                        recordsTotal = 0,

                        recordsFiltered = 0,

                        error =
                            "Invalid user."
                    },
                    JsonRequestBehavior.AllowGet);
            }


            if (user.PermissionGroup[PERM_VIEW] != '1')
            {
                return Json(
                    new
                    {
                        data =
                            new List<InvDepartmentRequisitionInfo>(),

                        draw = Request["draw"],

                        recordsTotal = 0,

                        recordsFiltered = 0,

                        error =
                            "You are not authorized."
                    },
                    JsonRequestBehavior.AllowGet);
            }


            try
            {
                InvDepartmentTransactionIssueResponse response =
                    InvDepartmentTransactionIssueDAL
                        .GetFilteredApprovedRequisitions(
                            Request,
                            loggedInUser);


                return Json(
                    new
                    {
                        data =
                            response.Requisitions,

                        draw =
                            Request["draw"],

                        recordsTotal =
                            response.TotalRecordCount,

                        recordsFiltered =
                            response.FilteredRecordCount
                    },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        data =
                            new List<InvDepartmentRequisitionInfo>(),

                        draw =
                            Request["draw"],

                        recordsTotal = 0,

                        recordsFiltered = 0,

                        error =
                            ex.GetBaseException().Message
                    },
                    JsonRequestBehavior.AllowGet);
            }
        }


        // =========================================================
        // ISSUE - GET
        // =========================================================

        [HttpGet]
        public ActionResult Issue(string id)
        {
            try
            {
                string loggedInUser =
                    Session["LoggedInUser"] as string;

                if (string.IsNullOrWhiteSpace(loggedInUser))
                {
                    return RedirectToAction(
                        "Logout",
                        "Login");
                }


                UserInfo user =
                    UserDAL.GetUserByUserCode(loggedInUser);

                if (user == null)
                {
                    return RedirectToAction(
                        "Logout",
                        "Login");
                }

                
                if (user.PermissionGroup[PERM_VIEW] != '1')
                {
                    TempData["ErrorMessage"] =
                        "You are not authorized to issue requisition.";

                    return RedirectToAction("Index");
                }


                if (string.IsNullOrWhiteSpace(id))
                {
                    id =
                        Request.QueryString["id"];
                }


                if (string.IsNullOrWhiteSpace(id))
                {
                    id =
                        Convert.ToString(
                            RouteData.Values["id"]);
                }


                if (string.IsNullOrWhiteSpace(id))
                {
                    TempData["ErrorMessage"] =
                        "Requisition ID is missing.";

                    return RedirectToAction("Index");
                }


                InvDepartmentRequisitionInfo requisition =
                    InvDepartmentTransactionIssueDAL
                        .GetRequisitionByID(id);


                if (requisition == null)
                {
                    TempData["ErrorMessage"] =
                        "Approved requisition was not found " +
                        "or it has already been issued.";

                    return RedirectToAction("Index");
                }


                if (requisition.Status != 2 ||
                    requisition.IsApproved != 1 ||
                    requisition.IsIssued == 1)
                {
                    TempData["ErrorMessage"] =
                        "Only approved and not-yet-issued " +
                        "requisitions can be issued.";

                    return RedirectToAction("Index");
                }


                ViewBag.User =
                    user;

                ViewBag.Requisition =
                    requisition;

                ViewBag.DepartmentList =
                    HrDepartmentDAL.GetAllHrDepartments();

                ViewBag.UnitList =
                    InvDepartmentTransactionIssueDAL
                        .GetUnitList();


                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    "Unable to load Issue page: " +
                    ex.GetBaseException().Message;

                return RedirectToAction("Index");
            }
        }


        // =========================================================
        // GET UNIT CONVERSION
        //
        // This is used by the Issue View for live calculation.
        // =========================================================

        [HttpGet]
        public JsonResult GetUnitConversion(
            string requestedUnitId,
            string issueUnitId,
            decimal unitPriceEst,
            decimal issueQuantity)
        {
            try
            {
                string loggedInUser =
                    Session["LoggedInUser"] as string;

                if (string.IsNullOrWhiteSpace(loggedInUser))
                {
                    return Json(
                        new
                        {
                            success = false,
                            message = "Your session has expired."
                        },
                        JsonRequestBehavior.AllowGet);
                }


                UserInfo user =
                    UserDAL.GetUserByUserCode(loggedInUser);

                if (user == null)
                {
                    return Json(
                        new
                        {
                            success = false,
                            message = "Invalid user."
                        },
                        JsonRequestBehavior.AllowGet);
                }


                if (user.PermissionGroup[PERM_VIEW] != '1')
                {
                    return Json(
                        new
                        {
                            success = false,
                            message = "You are not authorized."
                        },
                        JsonRequestBehavior.AllowGet);
                }


                if (string.IsNullOrWhiteSpace(
                    requestedUnitId))
                {
                    return Json(
                        new
                        {
                            success = false,
                            message = "Requested Unit is required."
                        },
                        JsonRequestBehavior.AllowGet);
                }


                if (string.IsNullOrWhiteSpace(
                    issueUnitId))
                {
                    return Json(
                        new
                        {
                            success = false,
                            message = "Issue Unit is required."
                        },
                        JsonRequestBehavior.AllowGet);
                }


                if (unitPriceEst < 0)
                {
                    return Json(
                        new
                        {
                            success = false,
                            message = "Invalid unit price."
                        },
                        JsonRequestBehavior.AllowGet);
                }


                if (issueQuantity < 0)
                {
                    issueQuantity = 0;
                }


                InvDepartmentTransactionIssueConversionInfo result =
                    InvDepartmentTransactionIssueDAL
                        .CalculateUnitConversion(
                            requestedUnitId,
                            issueUnitId,
                            unitPriceEst,
                            issueQuantity);


                if (result == null ||
                    !result.Success)
                {
                    return Json(
                        new
                        {
                            success = false,

                            message =
                                result == null
                                    ? "Unit conversion was not found."
                                    : result.Message
                        },
                        JsonRequestBehavior.AllowGet);
                }


                return Json(
                    new
                    {
                        success = true,

                        sameUnit =
                            result.SameUnit,

                        unitConversionId =
                            result.UnitConversionId,

                        conversionRate =
                            result.ConversionRate,

                        conversionPrice =
                            result.ConversionPrice,

                        conversionTotal =
                            result.ConversionTotal,

                        requestedEquivalentQuantity =
                            result.RequestedEquivalentQuantity
                    },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        success = false,

                        message =
                            ex.GetBaseException().Message
                    },
                    JsonRequestBehavior.AllowGet);
            }
        }


        // =========================================================
        // ISSUE - POST
        // =========================================================

        [HttpPost]
        public JsonResult Issue(
            string id,
            string fromDepartmentId,
            string toDepartmentId,
            string remarks,
            string IssuedLinesJson)
        {
            try
            {
                string loggedInUser =
                    Session["LoggedInUser"] as string;

                if (string.IsNullOrWhiteSpace(loggedInUser))
                {
                    return Json(
                        new
                        {
                            ResponseStatus = -1,
                            ResponseMessage =
                                "Your session has expired. " +
                                "Please login again."
                        });
                }


                UserInfo user =
                    UserDAL.GetUserByUserCode(loggedInUser);

                if (user == null)
                {
                    return Json(
                        new
                        {
                            ResponseStatus = -1,
                            ResponseMessage =
                                "Invalid user."
                        });
                }


                if (user.PermissionGroup[PERM_VIEW] != '1')
                {
                    return Json(
                        new
                        {
                            ResponseStatus = -2,
                            ResponseMessage =
                                "You are not authorized to issue requisition."
                        });
                }


                if (string.IsNullOrWhiteSpace(id))
                {
                    return Json(
                        new
                        {
                            ResponseStatus = 0,
                            ResponseMessage =
                                "Requisition ID is required."
                        });
                }


                if (string.IsNullOrWhiteSpace(
                    fromDepartmentId))
                {
                    return Json(
                        new
                        {
                            ResponseStatus = 0,
                            ResponseMessage =
                                "From Department is required."
                        });
                }


                if (string.IsNullOrWhiteSpace(
                    toDepartmentId))
                {
                    return Json(
                        new
                        {
                            ResponseStatus = 0,
                            ResponseMessage =
                                "To Department is required."
                        });
                }


                if (string.IsNullOrWhiteSpace(
                    IssuedLinesJson))
                {
                    return Json(
                        new
                        {
                            ResponseStatus = 0,
                            ResponseMessage =
                                "Issue details are required."
                        });
                }


                JavaScriptSerializer serializer =
                    new JavaScriptSerializer();


                List<InvDepartmentRequisitionIssuedLine>
                    issuedLines;


                try
                {
                    issuedLines =
                        serializer.Deserialize<
                            List<InvDepartmentRequisitionIssuedLine>
                        >(IssuedLinesJson);
                }
                catch
                {
                    return Json(
                        new
                        {
                            ResponseStatus = 0,
                            ResponseMessage =
                                "Invalid issue details."
                        });
                }


                if (issuedLines == null ||
                    issuedLines.Count == 0)
                {
                    return Json(
                        new
                        {
                            ResponseStatus = 0,
                            ResponseMessage =
                                "No issue items were submitted."
                        });
                }


                foreach (
                    InvDepartmentRequisitionIssuedLine line
                    in issuedLines)
                {
                    if (line == null ||
                        string.IsNullOrWhiteSpace(
                            line.RowId))
                    {
                        return Json(
                            new
                            {
                                ResponseStatus = 0,
                                ResponseMessage =
                                    "Invalid requisition detail RowId."
                            });
                    }


                    if (line.IssuedQuantity < 0)
                    {
                        return Json(
                            new
                            {
                                ResponseStatus = 0,
                                ResponseMessage =
                                    "Issued quantity cannot be negative."
                            });
                    }


                    if (line.IssuedQuantity > 0 &&
                        string.IsNullOrWhiteSpace(
                            line.IssueUnitId))
                    {
                        return Json(
                            new
                            {
                                ResponseStatus = 0,
                                ResponseMessage =
                                    "Issue Unit is required."
                            });
                    }
                }


                string message;


                bool success =
                    InvDepartmentTransactionIssueDAL
                        .IssueRequisition(
                            id,
                            fromDepartmentId,
                            toDepartmentId,
                            remarks,
                            issuedLines,
                            loggedInUser,
                            out message);


                return Json(
                    new
                    {
                        ResponseStatus =
                            success ? 1 : 0,

                        ResponseMessage =
                            message
                    });
            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        ResponseStatus = 0,

                        ResponseMessage =
                            "Issue failed: " +
                            ex.GetBaseException().Message
                    });
            }
        }
    }
}