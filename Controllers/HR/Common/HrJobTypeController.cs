using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ERP.Models.HR.Common.HrJobType;
using ERP.Models.User;
using ERP.DAL.HR.Common;
using ERP.DAL;

namespace ERP.Controllers.HR.Common
{

    public class HrJobTypeController : Controller
    {
        // GET: HrJobType

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
            ViewBag.HrJobTypes = HrJobTypeDAL.GetAllHrJobTypes();
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
                return Redirect(Url.Action("Index", "HrJobType"));
            }
            ViewBag.User = user;
            ViewBag.HrJobType = HrJobTypeDAL.GetAllHrJobTypes();
            ViewBag.HrJobTypes = HrJobTypeDAL.GetAllHrJobTypes();
            return View();
        }

        [HttpPost]
        public ActionResult Add(HrJobTypeInfo HrJobType)
        {
            HrJobTypeResponse response = new HrJobTypeResponse();
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

            HrJobType.RowId = Guid.NewGuid().ToString();
            if (string.IsNullOrWhiteSpace(HrJobType.JobType))
                response.ResponseMessages.Add("JobType Name is required");
            else if (HrJobTypeDAL.GetHrJobTypeByName(HrJobType.JobType, HrJobType.RowId) != null)
                response.ResponseMessages.Add("JobType Name already exists");

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";
                response.ResponseMessage = "<ul><li>" + response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(response);
            }

            HrJobType.RowId = Guid.NewGuid().ToString();
            try
            {
                HrJobTypeDAL.AddHrJobType(HrJobType, lgUser);
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
            response.ResponseMessage = "JobType is added successfully";
            response.HrJobType = HrJobType;
            return Json(response);
        }


        public ActionResult GetHrJobTypeItemList()
        {
            List<HrJobTypeInfo> HrJobTypeItems = new List<HrJobTypeInfo>();
            string lgUser = string.Empty;
            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();
            if (string.IsNullOrEmpty(lgUser))
            {
                return Json(HrJobTypeItems, JsonRequestBehavior.AllowGet);
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null)
            {
                return Json(HrJobTypeItems, JsonRequestBehavior.AllowGet);
            }
            if (user.PermissionGroup[0] != '1')
            {
                return Json(HrJobTypeItems, JsonRequestBehavior.AllowGet);
            }

            HrJobTypeResponse resp = HrJobTypeDAL.GetFilteredHrJobTypes(Request);


            return Json(new { data = resp.HrJobTypes, draw = Request["draw"], recordsTotal = resp.TotalRecordCount, recordsFiltered = resp.FilteredRecordCount }, JsonRequestBehavior.AllowGet);

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
                return Redirect(Url.Action("Index", "HrJobType"));
            }
            HrJobTypeInfo HrJobType = HrJobTypeDAL.GetJobTypeByID(RouteData.Values["id"].ToString());
            if (HrJobType == null)
            {
                return Redirect(Url.Action("Index", "HrJobType"));
            }
            ViewBag.User = user;
            ViewBag.HrJobTypes = HrJobTypeDAL.GetAllHrJobTypes();
            ViewBag.HrJobType = HrJobType;
            return View();
        }


        [HttpPost]
        public ActionResult Edit(HrJobTypeInfo HrJobType)
        {
            HrJobTypeResponse response = new HrJobTypeResponse();
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

            if (string.IsNullOrWhiteSpace(HrJobType.JobType))
                response.ResponseMessages.Add("JobType Name is required");
            else if (HrJobTypeDAL.GetHrJobTypeByName(HrJobType.JobType, HrJobType.RowId) != null)
                response.ResponseMessages.Add("JobType Name already exists");

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";
                response.ResponseMessage = "<ul><li>" + response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(response);
            }
            try
            {
                HrJobTypeDAL.UpdateHrJobType(HrJobType, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;
                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "JobType is added successfully";
            response.HrJobType = HrJobType;
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
                return Redirect(Url.Action("Index", "HrJobType"));
            }
            HrJobTypeInfo HrJobType = HrJobTypeDAL.GetJobTypeByID(RouteData.Values["id"].ToString());
            if (HrJobType == null)
            {
                return Redirect(Url.Action("Index", "HrJobType"));
            }
            ViewBag.User = user;
            ViewBag.HrJobType = HrJobType;
            return View();
        }

        [HttpPost]
        public ActionResult Delete(HrJobTypeInfo HrJobType)
        {
            HrJobTypeResponse response = new HrJobTypeResponse();
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
                HrJobTypeDAL.DeleteHrJobType(HrJobType, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;
                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "JobType is deleted successfully";
            response.HrJobType = HrJobType;
            return Json(response);
        } /**/
    }
}