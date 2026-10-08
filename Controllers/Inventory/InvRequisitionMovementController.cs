using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using ERP.DAL.Inventory;
using ERP.Models.Inventory.InvDepartmentRequisition;
using ERP.Models.Inventory.InvDepartmentRequisitionDetail;

namespace ERP.Controllers.Inventory
{
    public class InvRequisitionMovementController : Controller
    {
        // -------------------------------------------------
        // Status values used in your system
        // -------------------------------------------------
        private const int NEW = 0;
        private const int FORWARDED = 1;
        private const int APPROVED = 2;
        private const int REJECTED = 3;

        private bool IsLoggedIn()
        {
            return Session["LoggedInUser"] != null;
        }

        private string LgUser()
        {
            return Session["LoggedInUser"] != null
                ? Session["LoggedInUser"].ToString()
                : "";
        }

        private bool HasMovementPermission()
        {
            // TODO: replace with real permission check
            var perm = Session["Permission"] as string;
            return true;
        }

        // ========== Index ==========
        public ActionResult Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Logout", "Login");
            if (!HasMovementPermission()) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpGet]
        //public JsonResult GetList()
        //{
        //    if (!IsLoggedIn())
        //        return Json(new { ResponseStatus = -1 }, JsonRequestBehavior.AllowGet);

        //    string filter = Request["QueueFilter"]; // "New" | "Forwarded" | "All"

        //    int? statusFilter = null;   // null = all
        //    if (filter == "New")
        //        statusFilter = NEW;
        //    else if (filter == "Forwarded")
        //        statusFilter = FORWARDED;

            //var result = InvDepartmentRequisition2ndDAL.GetFilteredRequisitionsForMovement(Request, statusFilter);

            //return Json(new
            //{
            //    draw = Request["draw"],
            //    recordsTotal = result.TotalRecordCount,
            //    recordsFiltered = result.FilteredRecordCount,
            //    data = result.Requisitions
            //}, JsonRequestBehavior.AllowGet);
        //}

        // ---------- Forward ----------
        public ActionResult ForwardRequisition(string id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Logout", "Login");

            var req = InvDepartmentRequisitionDAL.GetRequisitionByID(id);
            if (req == null || req.Status != NEW)
                return RedirectToAction("Index");

            return View(req);
        }

        [HttpPost]
        public JsonResult ForwardRequisition(string id, string remarks)
        {
            try
            {
                if (!IsLoggedIn())
                    return Json(new { ResponseStatus = -1 });

                InvDepartmentRequisition2ndDAL.ForwardRequisition(id, LgUser(), remarks);
                return Json(new { ResponseStatus = 1, ResponseMessage = "Forwarded successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { ResponseStatus = 0, ResponseMessage = ex.Message });
            }
        }

        // ---------- Approve (full / partial) ----------
        public ActionResult ApprovedRequisition(string id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Logout", "Login");

            var req = InvDepartmentRequisitionDAL.GetRequisitionByID(id);
            if (req == null || req.Status != FORWARDED)
                return RedirectToAction("Index");

            // Default to full quantity if ApprovedQuantity is not set
            if (req.Details != null)
            {
                foreach (var d in req.Details)
                {
                    if (d.ApprovedQuantity <= 0)
                        d.ApprovedQuantity = d.Quantity;
                }
            }

            return View(req);
        }

        [HttpPost]
        public JsonResult ApprovedRequisition(string id, string remarks, string ApprovedLinesJson)
        {
            try
            {
                if (!IsLoggedIn())
                    return Json(new { ResponseStatus = -1 });

                var lines = string.IsNullOrEmpty(ApprovedLinesJson)
                    ? new List<InvDepartmentRequisitionDetailInfo>()
                    : JsonConvert.DeserializeObject<List<InvDepartmentRequisitionDetailInfo>>(ApprovedLinesJson);

                //InvDepartmentRequisition2ndDAL.ApproveRequisition(id, LgUser(), remarks, lines);
                return Json(new { ResponseStatus = 1, ResponseMessage = "Approved successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { ResponseStatus = 0, ResponseMessage = ex.Message });
            }
        }

        // ---------- Reject ----------
        public ActionResult RejectRequisition(string id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Logout", "Login");

            var req = InvDepartmentRequisition2ndDAL.GetRequisitionByID(id);
            if (req == null || req.Status != FORWARDED)
                return RedirectToAction("Index");

            return View(req);
        }

        [HttpPost]
        public JsonResult RejectRequisition(string id, string remarks)
        {
            try
            {
                if (!IsLoggedIn())
                    return Json(new { ResponseStatus = -1 });

                InvDepartmentRequisition2ndDAL.RejectRequisition(id, LgUser(), remarks);
                return Json(new { ResponseStatus = 1, ResponseMessage = "Rejected." });
            }
            catch (Exception ex)
            {
                return Json(new { ResponseStatus = 0, ResponseMessage = ex.Message });
            }
        }
    }
}