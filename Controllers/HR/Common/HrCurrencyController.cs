using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ERP.Models.HR.Common.HrCurrency;
using ERP.Models.User;
using ERP.DAL.HR.Common;
using ERP.DAL;
using Microsoft.Ajax.Utilities;

namespace ERP.Controllers.HR.Common
{
    public class HrCurrencyController : Controller
    {
        // GET: HrCurrency

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
            ViewBag.HrCurrencys = HrCurrencyDAL.GetAllHrCurrencys();
            ViewBag.User = user;
            return View();
        }

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
                return Redirect(Url.Action("Index", "HrCurrency"));
            }
            ViewBag.User = user;
            ViewBag.HrCurrency = HrCurrencyDAL.GetAllHrCurrencys();
            ViewBag.HrCountrys = HrCountryDAL.GetAllHrCountrys();
            return View();
        }

        [HttpPost]
        public ActionResult Add(HrCurrencyInfo HrCurrency)
        {
            HrCurrencyResponse response = new HrCurrencyResponse();
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
            HrCurrency.RowId = Guid.NewGuid().ToString();

            if (string.IsNullOrWhiteSpace(HrCurrency.CurrencyName))
                response.ResponseMessages.Add("Currency Name is required");
            else if (HrCurrencyDAL.GetHrCurrencyByName(HrCurrency.CurrencyName, HrCurrency.RowId) != null)
                response.ResponseMessages.Add("Currency Name already exists");

            if (string.IsNullOrWhiteSpace(HrCurrency.CurrencyCode))
                response.ResponseMessages.Add("Currency Code is required");
            else if (HrCurrencyDAL.GetHrCurrencyByCode(HrCurrency.CurrencyCode, HrCurrency.RowId) != null)
                response.ResponseMessages.Add("Currency Code already exists");

            if (string.IsNullOrEmpty(HrCurrency.CurrencyCode))
            {
                response.ResponseMessages.Add("Currency Code is required");
            }
            if (string.IsNullOrEmpty(HrCurrency.CurrencySymbol))
            {
                response.ResponseMessages.Add("Currency Symbol is required");
            }
            if (HrCurrency.Rank == null || HrCurrency.Rank <= 0)
            {
                response.ResponseMessages.Add("Rank is required and must be greater than 0");
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
                HrCurrencyDAL.AddHrCurrency(HrCurrency, lgUser);
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
            response.ResponseMessage = "Currency is added successfully";
            response.HrCurrency = HrCurrency;
            return Json(response);
        }


        public ActionResult GetHrCurrencyItemList()
        {
            List<HrCurrencyInfo> HrCurrencyItems = new List<HrCurrencyInfo>();
            string lgUser = string.Empty;
            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();
            if (string.IsNullOrEmpty(lgUser))
            {
                return Json(HrCurrencyItems, JsonRequestBehavior.AllowGet);
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null)
            {
                return Json(HrCurrencyItems, JsonRequestBehavior.AllowGet);
            }
            if (user.PermissionGroup[0] != '1')
            {
                return Json(HrCurrencyItems, JsonRequestBehavior.AllowGet);
            }

            HrCurrencyResponse resp = HrCurrencyDAL.GetFilteredHrCurrencys(Request);


            return Json(new { data = resp.HrCurrencys, draw = Request["draw"], recordsTotal = resp.TotalRecordCount, recordsFiltered = resp.FilteredRecordCount }, JsonRequestBehavior.AllowGet);

        }

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
                return Redirect(Url.Action("Index", "HrCurrency"));
            }
            HrCurrencyInfo HrCurrency = HrCurrencyDAL.GetCurrencyByID(RouteData.Values["id"].ToString());
            if (HrCurrency == null)
            {
                return Redirect(Url.Action("Index", "HrCurrency"));
            }
            ViewBag.User = user;
            //ViewBag.HrCurrencys = HrCurrencyDAL.GetAllHrCurrencys();
            ViewBag.HrCurrency = HrCurrency;
            ViewBag.HrCountrys = HrCountryDAL.GetAllHrCountrys();
            return View();
        }


        [HttpPost]
        public ActionResult Edit(HrCurrencyInfo HrCurrency)
        {
            HrCurrencyResponse response = new HrCurrencyResponse();
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

            if (string.IsNullOrWhiteSpace(HrCurrency.CurrencyName))
                response.ResponseMessages.Add("Currency Name is required");
            else if (HrCurrencyDAL.GetHrCurrencyByName(HrCurrency.CurrencyName, HrCurrency.RowId) != null)
                response.ResponseMessages.Add("Currency Name already exists");

            if (string.IsNullOrWhiteSpace(HrCurrency.CurrencyCode))
                response.ResponseMessages.Add("Currency Code is required");
            else if (HrCurrencyDAL.GetHrCurrencyByCode(HrCurrency.CurrencyCode, HrCurrency.RowId) != null)
                response.ResponseMessages.Add("Currency Code already exists");

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";
                response.ResponseMessage = "<ul><li>" + response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(response);
            }
            try
            {
                HrCurrencyDAL.UpdateHrCurrency(HrCurrency, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;
                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "Currency is added successfully";
            response.HrCurrency = HrCurrency;
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
                return Redirect(Url.Action("Index", "HrCurrency"));
            }
            HrCurrencyInfo HrCurrency = HrCurrencyDAL.GetCurrencyByID(RouteData.Values["id"].ToString());
            if (HrCurrency == null)
            {
                return Redirect(Url.Action("Index", "HrCurrency"));
            }
            ViewBag.User = user;
            ViewBag.HrCurrency = HrCurrency;
            return View();
        }

        [HttpPost]
        public ActionResult Delete(HrCurrencyInfo HrCurrency)
        {
            HrCurrencyResponse response = new HrCurrencyResponse();
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
                HrCurrencyDAL.DeleteHrCurrency(HrCurrency, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;
                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "Currency is deleted successfully";
            response.HrCurrency = HrCurrency;
            return Json(response);
        } /**/
    }
}