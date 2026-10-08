using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ERP.Models.HR.Common.HrCountry;
using ERP.Models.User;
using ERP.DAL.HR.Common;
using ERP.DAL;

namespace ERP.Controllers.HR.Common
{
    
        public class HrCountryController : Controller
        {
            // GET: HrCountry

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
                ViewBag.HrCountrys = HrCountryDAL.GetAllHrCountrys();
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
                    return Redirect(Url.Action("Index", "HrCountry"));
                }
                ViewBag.User = user;
                ViewBag.HrCountry = HrCountryDAL.GetAllHrCountrys();
                ViewBag.HrCountrys = HrCountryDAL.GetAllHrCountrys();
                return View();
            }

            [HttpPost]
            public ActionResult Add(HrCountryInfo HrCountry)
            {
                HrCountryResponse response = new HrCountryResponse();
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

            HrCountry.RowId = Guid.NewGuid().ToString();
            if (string.IsNullOrWhiteSpace(HrCountry.CountryName))
                response.ResponseMessages.Add("Country Name is required");
            else if (HrCountryDAL.GetHrCountryByName(HrCountry.CountryName, HrCountry.RowId) != null)
                response.ResponseMessages.Add("Country Name already exists");

            if (string.IsNullOrWhiteSpace(HrCountry.CountryCode))
                response.ResponseMessages.Add("Country Code is required");
            else if (HrCountryDAL.GetHrCountryByCode(HrCountry.CountryCode, HrCountry.RowId) != null)
                response.ResponseMessages.Add("Country Code already exists");

            if (string.IsNullOrEmpty(HrCountry.CountryCode))
            {
                response.ResponseMessages.Add("Country Code is required");
            }

            

            if (response.ResponseMessages.Count > 0)
                {
                    response.ResponseStatus = 0;
                    response.ResponseCode = "Invalid Data";
                    response.ResponseMessage = "<ul><li>" + response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                    return Json(response);
                }

                HrCountry.RowId = Guid.NewGuid().ToString();
                try
                {
                    HrCountryDAL.AddHrCountry(HrCountry, lgUser);
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
                response.ResponseMessage = "Country is added successfully";
                response.HrCountry = HrCountry;
                return Json(response);
            }


            public ActionResult GetHrCountryItemList()
            {
                List<HrCountryInfo> HrCountryItems = new List<HrCountryInfo>();
                string lgUser = string.Empty;
                if (Session["LoggedInUser"] != null)
                    lgUser = Session["LoggedInUser"].ToString();
                if (string.IsNullOrEmpty(lgUser))
                {
                    return Json(HrCountryItems, JsonRequestBehavior.AllowGet);
                }

                UserInfo user = UserDAL.GetUserByUserCode(lgUser);
                if (user == null)
                {
                    return Json(HrCountryItems, JsonRequestBehavior.AllowGet);
                }
                if (user.PermissionGroup[0] != '1')
                {
                    return Json(HrCountryItems, JsonRequestBehavior.AllowGet);
                }

                HrCountryResponse resp = HrCountryDAL.GetFilteredHrCountrys(Request);


                return Json(new { data = resp.HrCountrys, draw = Request["draw"], recordsTotal = resp.TotalRecordCount, recordsFiltered = resp.FilteredRecordCount }, JsonRequestBehavior.AllowGet);

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
                    return Redirect(Url.Action("Index", "HrCountry"));
                }
                HrCountryInfo HrCountry = HrCountryDAL.GetCountryByID(RouteData.Values["id"].ToString());
                if (HrCountry == null)
                {
                    return Redirect(Url.Action("Index", "HrCountry"));
                }
                ViewBag.User = user;
                ViewBag.HrCountrys = HrCountryDAL.GetAllHrCountrys();
                ViewBag.HrCountry = HrCountry;
                return View();
            }


            [HttpPost]
            public ActionResult Edit(HrCountryInfo HrCountry)
            {
                HrCountryResponse response = new HrCountryResponse();
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

            if (string.IsNullOrWhiteSpace(HrCountry.CountryName))
                response.ResponseMessages.Add("Country Name is required");
            else if (HrCountryDAL.GetHrCountryByName(HrCountry.CountryName, HrCountry.RowId) != null)
                response.ResponseMessages.Add("Country Name already exists");

            if (string.IsNullOrWhiteSpace(HrCountry.CountryCode))
                response.ResponseMessages.Add("Country Code is required");
            else if (HrCountryDAL.GetHrCountryByCode(HrCountry.CountryCode, HrCountry.RowId) != null)
                response.ResponseMessages.Add("Country Code already exists");

            if (response.ResponseMessages.Count > 0)
                {
                    response.ResponseStatus = 0;
                    response.ResponseCode = "Invalid Data";
                    response.ResponseMessage = "<ul><li>" + response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                    return Json(response);
                }
                try
                {
                    HrCountryDAL.UpdateHrCountry(HrCountry, lgUser);
                }
                catch (Exception ex)
                {
                    response.ResponseStatus = 0;
                    response.ResponseCode = "Database Error";
                    response.ResponseMessage = ex.Message;
                    return Json(response);
                }

                response.ResponseStatus = 1;
                response.ResponseMessage = "Country is added successfully";
                response.HrCountry = HrCountry;
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
                    return Redirect(Url.Action("Index", "HrCountry"));
                }
                HrCountryInfo HrCountry = HrCountryDAL.GetCountryByID(RouteData.Values["id"].ToString());
                if (HrCountry == null)
                {
                    return Redirect(Url.Action("Index", "HrCountry"));
                }
                ViewBag.User = user;
                ViewBag.HrCountry = HrCountry;
                return View();
            }

            [HttpPost]
            public ActionResult Delete(HrCountryInfo HrCountry)
            {
                HrCountryResponse response = new HrCountryResponse();
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
                    HrCountryDAL.DeleteHrCountry(HrCountry, lgUser);
                }
                catch (Exception ex)
                {
                    response.ResponseStatus = 0;
                    response.ResponseCode = "Database Error";
                    response.ResponseMessage = ex.Message;
                    return Json(response);
                }

                response.ResponseStatus = 1;
                response.ResponseMessage = "Country is deleted successfully";
                response.HrCountry = HrCountry;
                return Json(response);
            } /**/
        }

    }