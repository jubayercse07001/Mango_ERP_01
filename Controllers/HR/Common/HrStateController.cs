using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ERP.Models.HR.Common.HrState;
using ERP.Models.User;
using ERP.DAL.HR.Common;
using ERP.DAL;

namespace ERP.Controllers.HR.Common
{

    public class HrStateController : Controller
    {
        // GET: HrState

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
            ViewBag.HrStates = HrStateDAL.GetAllHrStates();
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
                return Redirect(Url.Action("Index", "HrState"));
            }
            ViewBag.User = user;
            //ViewBag.HrState = HrStateDAL.GetAllHrStates();
            ViewBag.HrStates = HrStateDAL.GetAllHrStates();
            ViewBag.HrCountrys = HrCountryDAL.GetAllHrCountrys();
            return View();
        }

        [HttpPost]
        public ActionResult Add(HrStateInfo HrState)
        {
            HrStateResponse response = new HrStateResponse();
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

            HrState.RowId = Guid.NewGuid().ToString();
            if (string.IsNullOrWhiteSpace(HrState.StateName))
                response.ResponseMessages.Add("State Name is required");
            else if (HrStateDAL.GetHrStateByName(HrState.StateName, HrState.RowId) != null)
                response.ResponseMessages.Add("State Name already exists");
            if (string.IsNullOrEmpty(HrState.CountryId))
            {
                response.ResponseMessages.Add("Country Name is required");
            }

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";
                response.ResponseMessage = "<ul><li>" + response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(response);
            }

            HrState.RowId = Guid.NewGuid().ToString();
            try
            {
                HrStateDAL.AddHrState(HrState, lgUser);
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
            response.ResponseMessage = "State is added successfully";
            response.HrState = HrState;
            return Json(response);
        }


        public ActionResult GetHrStateItemList()
        {
            List<HrStateInfo> HrStateItems = new List<HrStateInfo>();
            string lgUser = string.Empty;
            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();
            if (string.IsNullOrEmpty(lgUser))
            {
                return Json(HrStateItems, JsonRequestBehavior.AllowGet);
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null)
            {
                return Json(HrStateItems, JsonRequestBehavior.AllowGet);
            }
            if (user.PermissionGroup[0] != '1')
            {
                return Json(HrStateItems, JsonRequestBehavior.AllowGet);
            }

            HrStateResponse resp = HrStateDAL.GetFilteredHrStates(Request);


            return Json(new { data = resp.HrStates, draw = Request["draw"], recordsTotal = resp.TotalRecordCount, recordsFiltered = resp.FilteredRecordCount }, JsonRequestBehavior.AllowGet);

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
                return Redirect(Url.Action("Index", "HrState"));
            }
            HrStateInfo HrState = HrStateDAL.GetStateByID(RouteData.Values["id"].ToString());
            if (HrState == null)
            {
                return Redirect(Url.Action("Index", "HrState"));
            }
            ViewBag.User = user;
            //ViewBag.HrStates = HrStateDAL.GetAllHrStates();
            ViewBag.HrState = HrState;
            ViewBag.HrCountrys = HrCountryDAL.GetAllHrCountrys();
            return View();
        }


        [HttpPost]
        public ActionResult Edit(HrStateInfo HrState)
        {
            HrStateResponse response = new HrStateResponse();
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

            if (string.IsNullOrWhiteSpace(HrState.StateName))
                response.ResponseMessages.Add("State Name is required");
            else if (HrStateDAL.GetHrStateByName(HrState.StateName, HrState.RowId) != null)
                response.ResponseMessages.Add("State Name already exists");

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";
                response.ResponseMessage = "<ul><li>" + response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(response);
            }
            try
            {
                HrStateDAL.UpdateHrState(HrState, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;
                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "State is added successfully";
            response.HrState = HrState;
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
                return Redirect(Url.Action("Index", "HrState"));
            }
            HrStateInfo HrState = HrStateDAL.GetStateByID(RouteData.Values["id"].ToString());
            if (HrState == null)
            {
                return Redirect(Url.Action("Index", "HrState"));
            }
            ViewBag.User = user;
            ViewBag.HrState = HrState;
            return View();
        }

        [HttpPost]
        public ActionResult Delete(HrStateInfo HrState)
        {
            HrStateResponse response = new HrStateResponse();
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
                HrStateDAL.DeleteHrState(HrState, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;
                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "State is deleted successfully";
            response.HrState = HrState;
            return Json(response);
        } /**/
    }

}