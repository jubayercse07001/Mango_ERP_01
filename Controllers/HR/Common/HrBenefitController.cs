using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ERP.Models.HR.Common.HrBenefit;
using ERP.Models.User;
using ERP.DAL.HR.Common;
using ERP.DAL;

namespace ERP.Controllers.HR.Common
{
    public class HrBenefitController : Controller
    {
        // GET: HrBenefit
        
        public ActionResult Index()
        {
            string lgUser = string.Empty;
            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();
            if (string.IsNullOrEmpty(lgUser))
            {
                return Redirect(Url.Action("Logout", "Login"));
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null)
            {
                return Redirect(Url.Action("Logout", "Login"));
            }
            if (user.PermissionGroup[0] != '1')
            {
                return Redirect(Url.Action("Index", "Home"));
            }
            //ViewBag.HrBenefits = HrBenefitDAL.GetAllHrBenefits();
            //ViewBag.Banks = BankDAL.GetAllBanks();
            ViewBag.User = user;
            return View();
        }
/*
        [HttpGet]
        public ActionResult Add()
        {
            string lgUser = string.Empty;
            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();
            if (string.IsNullOrEmpty(lgUser))
            {
                return Redirect(Url.Action("Logout", "Login"));
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null)
            {
                return Redirect(Url.Action("Logout", "Login"));
            }
            if (user.PermissionGroup[1] != '1')
            {
                return Redirect(Url.Action("Index", "HrBenefit"));
            }
            ViewBag.User = user;
            ViewBag.Banks = BankDAL.GetAllBanks();
            ViewBag.Branchs = BranchDAL.GetAllBranchs();
            //ViewBag.Branchs = BranchDAL.GetAllBranchByBankId(bankId);
            ViewBag.HrBenefits = HrBenefitDAL.GetAllHrBenefits();
            return View();
        }

        [HttpPost]
        public ActionResult Add(HrBenefitInfo HrBenefit)
        {
            HrBenefitResponse response = new HrBenefitResponse();
            string lgUser = string.Empty;
            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();
            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                response.ResponseCode = "Not Looged In";
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

            if (string.IsNullOrEmpty(HrBenefit.BankID))
            {
                response.ResponseMessages.Add("Bank is required");
            }

            if (string.IsNullOrEmpty(HrBenefit.BranchID))
            {
                response.ResponseMessages.Add("Branch is required");
            }
            if (string.IsNullOrEmpty(HrBenefit.AccountTypeID))
            {
                response.ResponseMessages.Add("Account Type is required");
            }
            if (string.IsNullOrEmpty(HrBenefit.AccountName))
            {
                response.ResponseMessages.Add("Account Name is required");
            }

            if (string.IsNullOrEmpty(HrBenefit.AccountShortCode))
            {
                response.ResponseMessages.Add("Account Short Code is required");
            }

            if (!string.IsNullOrEmpty(HrBenefit.AccountName))
            {
                if (HrBenefitDAL.GetHrBenefitByAccountName(HrBenefit.AccountName) != null)
                {
                    response.ResponseMessages.Add("Bank Account Name  already exists");
                }
            }

            //if (string.IsNullOrEmpty(department.DepartmentType))
            //{
            //    response.ResponseMessages.Add("Department Type is required");
            //}

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";
                response.ResponseMessage = "<ul><li>" + response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(response);
            }

            HrBenefit.HrBenefitID = Guid.NewGuid().ToString();
            try
            {
                HrBenefitDAL.AddHrBenefit(HrBenefit, lgUser);
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
            response.ResponseMessage = "Bank Account is added successfully";
            response.HrBenefit = HrBenefit;
            return Json(response);
        }  */


        public ActionResult GetHrBenefitItemList()
        {
            List<HrBenefitInfo> HrBenefitItems = new List<HrBenefitInfo>();
            string lgUser = string.Empty;
            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();
            if (string.IsNullOrEmpty(lgUser))
            {
                return Json(HrBenefitItems, JsonRequestBehavior.AllowGet);
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null)
            {
                return Json(HrBenefitItems, JsonRequestBehavior.AllowGet);
            }
            if (user.PermissionGroup[0] != '1')
            {
                return Json(HrBenefitItems, JsonRequestBehavior.AllowGet);
            }

            HrBenefitResponse resp = HrBenefitDAL.GetFilteredHrBenefits(Request);


            return Json(new { data = resp.HrBenefits, draw = Request["draw"], recordsTotal = resp.TotalRecordCount, recordsFiltered = resp.FilteredRecordCount }, JsonRequestBehavior.AllowGet);

        }
        /*
        [HttpGet]
        public ActionResult Edit()
        {
            string lgUser = string.Empty;
            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();
            if (string.IsNullOrEmpty(lgUser))
            {
                return Redirect(Url.Action("Logout", "Login"));
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null)
            {
                return Redirect(Url.Action("Logout", "Login"));
            }
            if (user.PermissionGroup[2] != '1')
            {
                return Redirect(Url.Action("Index", "HrBenefit"));
            }
            HrBenefitInfo HrBenefit = HrBenefitDAL.GetHrBenefitByHrBenefitID(RouteData.Values["id"].ToString());
            if (HrBenefit == null)
            {
                return Redirect(Url.Action("Index", "HrBenefit"));
            }
            ViewBag.User = user;
            ViewBag.Banks = BankDAL.GetAllBanks();
            ViewBag.Branchs = BranchDAL.GetAllBranchs();
            ViewBag.HrBenefit = HrBenefit;
            return View();
        }


        [HttpPost]
        public ActionResult Edit(HrBenefitInfo HrBenefit)
        {
            HrBenefitResponse response = new HrBenefitResponse();
            string lgUser = string.Empty;
            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();
            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                response.ResponseCode = "Not Looged In";
                return Json(response);
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null)
            {
                response.ResponseStatus = -1;
                response.ResponseCode = "Invalid User";
                return Json(response);
            }
            if (user.PermissionGroup[2] != '1')
            {
                response.ResponseStatus = -2;
                response.ResponseCode = "Not Authorized";
                return Json(response);
            }

            if (string.IsNullOrEmpty(HrBenefit.AccountName))
            {
                response.ResponseMessages.Add("HrBenefit Name is required");
            }
            else if (HrBenefitDAL.GetHrBenefitByAccountName(HrBenefit.AccountName) != null)
            {
                response.ResponseMessages.Add("HrBenefit Name  already exists");
            }

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";
                response.ResponseMessage = "<ul><li>" + response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(response);
            }
            try
            {
                HrBenefitDAL.UpdateHrBenefit(HrBenefit, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;
                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "Bank Account is added successfully";
            response.HrBenefit = HrBenefit;
            return Json(response);
        }


        public ActionResult Delete()
        {
            string lgUser = string.Empty;
            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();
            if (string.IsNullOrEmpty(lgUser))
            {
                return Redirect(Url.Action("Logout", "Login"));
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null)
            {
                return Redirect(Url.Action("Logout", "Login"));
            }
            if (user.PermissionGroup[3] != '1')
            {
                return Redirect(Url.Action("Index", "HrBenefit"));
            }
            HrBenefitInfo HrBenefit = HrBenefitDAL.GetHrBenefitByHrBenefitID(RouteData.Values["id"].ToString());
            if (HrBenefit == null)
            {
                return Redirect(Url.Action("Index", "HrBenefit"));
            }
            ViewBag.User = user;
            ViewBag.HrBenefit = HrBenefit;

            return View();
        }

        [HttpPost]
        public ActionResult Delete(HrBenefitInfo HrBenefit)
        {
            HrBenefitResponse response = new HrBenefitResponse();
            string lgUser = string.Empty;
            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();
            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                response.ResponseCode = "Not Looged In";
                return Json(response);
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
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


            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";
                response.ResponseMessage = "<ul><li>" + response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(response);
            }

            try
            {
                HrBenefitDAL.DeleteHrBenefit(HrBenefit, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;
                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "Bank Account is deleted successfully";
            response.HrBenefit = HrBenefit;
            return Json(response);
        } */
    }
}