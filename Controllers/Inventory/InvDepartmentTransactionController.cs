using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using ERP.DAL;
using ERP.DAL.Inventory;
using ERP.DAL.HR.Organization;

using ERP.Models.User;
using ERP.Models.Inventory.InvDepartmentTransaction;
using ERP.Models.Inventory.InvDepartmentTransactionDetail;
using ERP.DAL.HR.Common;

namespace ERP.Controllers.Inventory
{
    public class InvDepartmentTransactionController
        : Controller
    {
        // ============================================================
        // INDEX
        // ============================================================

        public ActionResult Index()
        {
            string lgUser =
                Session["LoggedInUser"]?.ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Redirect(
                    Url.Action("Logout", "Login"));


            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);


            if (user == null)
                return Redirect(
                    Url.Action("Logout", "Login"));


            if (user.PermissionGroup[0] != '1')
                return Redirect(
                    Url.Action("Index", "Home"));


            ViewBag.User = user;


            ViewBag.DepartmentList =HrDepartmentDAL.GetAllHrDepartments();


            return View();
        }


        // ============================================================
        // DATATABLE
        // ============================================================

        public ActionResult
            GetInvDepartmentTransactionList()
        {
            try
            {
                var response =
                    InvDepartmentTransactionDAL
                    .GetFilteredTransactions(
                        Request);


                return Json(
                    new
                    {
                        data =
                            response
                            .InvDepartmentTransactions,

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
                            new List<
                                InvDepartmentTransactionInfo>(),

                        draw =
                            Request["draw"],

                        recordsTotal = 0,

                        recordsFiltered = 0,

                        error = ex.Message
                    },
                    JsonRequestBehavior.AllowGet);
            }
        }


        // ============================================================
        // DETAILS / EDIT GET
        // ============================================================

        [HttpGet]
        public ActionResult Edit()
        {
            string lgUser =
                Session["LoggedInUser"]?.ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Redirect(
                    Url.Action("Logout", "Login"));


            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);


            if (user == null)
                return Redirect(
                    Url.Action("Logout", "Login"));


            if (user.PermissionGroup[2] != '1')
                return Redirect(
                    Url.Action(
                        "Index",
                        "InvDepartmentTransaction"));


            int transactionId;

            if (!int.TryParse(
                RouteData.Values["id"]?.ToString(),
                out transactionId))
            {
                return Redirect(
                    Url.Action(
                        "Index",
                        "InvDepartmentTransaction"));
            }


            var model =
                InvDepartmentTransactionDAL
                .GetTransactionByID(
                    transactionId);


            if (model == null)
                return Redirect(
                    Url.Action(
                        "Index",
                        "InvDepartmentTransaction"));


            ViewBag.User = user;

            ViewBag.Transaction = model;

            ViewBag.Details =
                InvDepartmentTransactionDAL
                .GetTransactionDetails(
                    transactionId);


            ViewBag.DepartmentList =
                HrDepartmentDAL
                .GetAllHrDepartments();


            return View();
        }


        // ============================================================
        // EDIT POST
        // ============================================================

        [HttpPost]
        public ActionResult Edit(
            InvDepartmentTransactionInfo model)
        {
            var response =
                new InvDepartmentTransactionResponse();


            string lgUser =
                Session["LoggedInUser"]?.ToString();


            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;

                return Json(response);
            }


            try
            {
                InvDepartmentTransactionDAL
                    .UpdateTransaction(
                        model,
                        lgUser);


                response.ResponseStatus = 1;

                response.ResponseMessage =
                    "Transaction updated successfully.";
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;

                response.ResponseMessage =
                    ex.Message;
            }


            return Json(response);
        }


        // ============================================================
        // FORWARD
        // ============================================================

        [HttpPost]
        public ActionResult Forward(
            int TransactionId,
            string Remarks)
        {
            var response =
                new InvDepartmentTransactionResponse();


            string lgUser =
                Session["LoggedInUser"]?.ToString();


            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;

                return Json(response);
            }


            try
            {
                InvDepartmentTransactionDAL
                    .ForwardTransaction(
                        TransactionId,
                        lgUser,
                        Remarks);


                response.ResponseStatus = 1;

                response.ResponseMessage =
                    "Requisition forwarded successfully.";
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;

                response.ResponseMessage =
                    ex.Message;
            }


            return Json(response);
        }


        // ============================================================
        // APPROVE
        // ============================================================

        [HttpPost]
        public ActionResult Approve(
            int TransactionId,
            string Remarks)
        {
            var response =
                new InvDepartmentTransactionResponse();


            string lgUser =
                Session["LoggedInUser"]?.ToString();


            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;

                return Json(response);
            }


            try
            {
                InvDepartmentTransactionDAL
                    .ApproveTransaction(
                        TransactionId,
                        lgUser,
                        Remarks);


                response.ResponseStatus = 1;

                response.ResponseMessage =
                    "Requisition approved successfully.";
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;

                response.ResponseMessage =
                    ex.Message;
            }


            return Json(response);
        }


        // ============================================================
        // REJECT
        // ============================================================

        [HttpPost]
        public ActionResult Reject(
            int TransactionId,
            string Remarks)
        {
            var response =
                new InvDepartmentTransactionResponse();


            string lgUser =
                Session["LoggedInUser"]?.ToString();


            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;

                return Json(response);
            }


            try
            {
                InvDepartmentTransactionDAL
                    .RejectTransaction(
                        TransactionId,
                        lgUser,
                        Remarks);


                response.ResponseStatus = 1;

                response.ResponseMessage =
                    "Requisition rejected successfully.";
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;

                response.ResponseMessage =
                    ex.Message;
            }


            return Json(response);
        }


        // ============================================================
        // DELETE
        // ============================================================

        [HttpGet]
        public ActionResult Delete()
        {
            string lgUser =
                Session["LoggedInUser"]?.ToString();


            if (string.IsNullOrEmpty(lgUser))
                return Redirect(
                    Url.Action("Logout", "Login"));


            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);


            if (user == null)
                return Redirect(
                    Url.Action("Logout", "Login"));


            if (user.PermissionGroup[3] != '1')
                return Redirect(
                    Url.Action(
                        "Index",
                        "InvDepartmentTransaction"));


            int transactionId;

            if (!int.TryParse(
                RouteData.Values["id"]?.ToString(),
                out transactionId))
            {
                return Redirect(
                    Url.Action(
                        "Index",
                        "InvDepartmentTransaction"));
            }


            var model =
                InvDepartmentTransactionDAL
                .GetTransactionByID(
                    transactionId);


            if (model == null)
                return Redirect(
                    Url.Action(
                        "Index",
                        "InvDepartmentTransaction"));


            ViewBag.User =
                user;

            ViewBag.Transaction =
                model;


            return View();
        }


        [HttpPost]
        public ActionResult Delete(
            InvDepartmentTransactionInfo model)
        {
            var response =
                new InvDepartmentTransactionResponse();


            string lgUser =
                Session["LoggedInUser"]?.ToString();


            try
            {
                InvDepartmentTransactionDAL
                    .DeleteTransaction(
                        model.TransactionId,
                        lgUser);


                response.ResponseStatus = 1;

                response.ResponseMessage =
                    "Transaction deleted successfully.";
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;

                response.ResponseMessage =
                    ex.Message;
            }


            return Json(response);
        }
    }
}