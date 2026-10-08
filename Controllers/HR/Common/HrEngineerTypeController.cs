using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ERP.Models.HR.Common.HrEngineerType;
using ERP.Models.User;
using ERP.DAL.HR.Common;
using ERP.DAL;

namespace ERP.Controllers.HR.Common
{
   
    public class HrEngineerTypeController : Controller
    {
        // GET: HrEngineerType

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
            ViewBag.HrEngineerTypes = HrEngineerTypeDAL.GetAllHrEngineerTypes();
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
                return Redirect(Url.Action("Index", "HrEngineerType"));
            }
            ViewBag.User = user;
            ViewBag.HrEngineerType = HrEngineerTypeDAL.GetAllHrEngineerTypes();
            ViewBag.HrEngineerTypes = HrEngineerTypeDAL.GetAllHrEngineerTypes();
            return View();
        }

        [HttpPost]
        public ActionResult Add(HrEngineerTypeInfo HrEngineerType)
        {
            HrEngineerTypeResponse response = new HrEngineerTypeResponse();
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

            HrEngineerType.RowId = Guid.NewGuid().ToString();
            if (string.IsNullOrWhiteSpace(HrEngineerType.EngineerTypeName))
                response.ResponseMessages.Add("EngineerType Name is required");
            else if (HrEngineerTypeDAL.GetHrEngineerTypeByName(HrEngineerType.EngineerTypeName, HrEngineerType.RowId) != null)
                response.ResponseMessages.Add("EngineerType Name already exists");

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";
                response.ResponseMessage = "<ul><li>" + response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(response);
            }

            HrEngineerType.RowId = Guid.NewGuid().ToString();
            try
            {
                HrEngineerTypeDAL.AddHrEngineerType(HrEngineerType, lgUser);
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
            response.ResponseMessage = "EngineerType is added successfully";
            response.HrEngineerType = HrEngineerType;
            return Json(response);
        }


        public ActionResult GetHrEngineerTypeItemList()
        {
            List<HrEngineerTypeInfo> HrEngineerTypeItems = new List<HrEngineerTypeInfo>();
            string lgUser = string.Empty;
            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();
            if (string.IsNullOrEmpty(lgUser))
            {
                return Json(HrEngineerTypeItems, JsonRequestBehavior.AllowGet);
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null)
            {
                return Json(HrEngineerTypeItems, JsonRequestBehavior.AllowGet);
            }
            if (user.PermissionGroup[0] != '1')
            {
                return Json(HrEngineerTypeItems, JsonRequestBehavior.AllowGet);
            }

            HrEngineerTypeResponse resp = HrEngineerTypeDAL.GetFilteredHrEngineerTypes(Request);


            return Json(new { data = resp.HrEngineerTypes, draw = Request["draw"], recordsTotal = resp.TotalRecordCount, recordsFiltered = resp.FilteredRecordCount }, JsonRequestBehavior.AllowGet);

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
                return Redirect(Url.Action("Index", "HrEngineerType"));
            }
            HrEngineerTypeInfo HrEngineerType = HrEngineerTypeDAL.GetEngineerTypeByID(RouteData.Values["id"].ToString());
            if (HrEngineerType == null)
            {
                return Redirect(Url.Action("Index", "HrEngineerType"));
            }
            ViewBag.User = user;
            ViewBag.HrEngineerTypes = HrEngineerTypeDAL.GetAllHrEngineerTypes();
            ViewBag.HrEngineerType = HrEngineerType;
            return View();
        }


        [HttpPost]
        public ActionResult Edit(HrEngineerTypeInfo HrEngineerType)
        {
            HrEngineerTypeResponse response = new HrEngineerTypeResponse();
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

            if (string.IsNullOrWhiteSpace(HrEngineerType.EngineerTypeName))
                response.ResponseMessages.Add("EngineerType Name is required");
            else if (HrEngineerTypeDAL.GetHrEngineerTypeByName(HrEngineerType.EngineerTypeName, HrEngineerType.RowId) != null)
                response.ResponseMessages.Add("EngineerType Name already exists");

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";
                response.ResponseMessage = "<ul><li>" + response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(response);
            }
            try
            {
                HrEngineerTypeDAL.UpdateHrEngineerType(HrEngineerType, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;
                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "EngineerType is added successfully";
            response.HrEngineerType = HrEngineerType;
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
                return Redirect(Url.Action("Index", "HrEngineerType"));
            }
            HrEngineerTypeInfo HrEngineerType = HrEngineerTypeDAL.GetEngineerTypeByID(RouteData.Values["id"].ToString());
            if (HrEngineerType == null)
            {
                return Redirect(Url.Action("Index", "HrEngineerType"));
            }
            ViewBag.User = user;
            ViewBag.HrEngineerType = HrEngineerType;
            return View();
        }

        [HttpPost]
        public ActionResult Delete(HrEngineerTypeInfo HrEngineerType)
        {
            HrEngineerTypeResponse response = new HrEngineerTypeResponse();
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
                HrEngineerTypeDAL.DeleteHrEngineerType(HrEngineerType, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;
                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "EngineerType is deleted successfully";
            response.HrEngineerType = HrEngineerType;
            return Json(response);
        } /**/
    }
}