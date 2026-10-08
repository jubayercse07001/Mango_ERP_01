using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ERP.Models.HR.Common.HrSkillType;
using ERP.Models.User;
using ERP.DAL.HR.Common;
using ERP.DAL;

namespace ERP.Controllers.HR.Common
{

    public class HrSkillTypeController : Controller
    {
        // GET: HrSkillType

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
            ViewBag.HrSkillTypes = HrSkillTypeDAL.GetAllHrSkillTypes();
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
                return Redirect(Url.Action("Index", "HrSkillType"));
            }
            ViewBag.User = user;
            ViewBag.HrSkillType = HrSkillTypeDAL.GetAllHrSkillTypes();
            ViewBag.HrSkillTypes = HrSkillTypeDAL.GetAllHrSkillTypes();
            return View();
        }

        [HttpPost]
        public ActionResult Add(HrSkillTypeInfo HrSkillType)
        {
            HrSkillTypeResponse response = new HrSkillTypeResponse();
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

            HrSkillType.RowId = Guid.NewGuid().ToString();
            if (string.IsNullOrWhiteSpace(HrSkillType.SkillTypeName))
                response.ResponseMessages.Add("SkillType Name is required");
            else if (HrSkillTypeDAL.GetHrSkillTypeByName(HrSkillType.SkillTypeName, HrSkillType.RowId) != null)
                response.ResponseMessages.Add("SkillType Name already exists");


            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";
                response.ResponseMessage = "<ul><li>" + response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(response);
            }

            HrSkillType.RowId = Guid.NewGuid().ToString();
            try
            {
                HrSkillTypeDAL.AddHrSkillType(HrSkillType, lgUser);
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
            response.ResponseMessage = "SkillType is added successfully";
            response.HrSkillType = HrSkillType;
            return Json(response);
        }


        public ActionResult GetHrSkillTypeItemList()
        {
            List<HrSkillTypeInfo> HrSkillTypeItems = new List<HrSkillTypeInfo>();
            string lgUser = string.Empty;
            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();
            if (string.IsNullOrEmpty(lgUser))
            {
                return Json(HrSkillTypeItems, JsonRequestBehavior.AllowGet);
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null)
            {
                return Json(HrSkillTypeItems, JsonRequestBehavior.AllowGet);
            }
            if (user.PermissionGroup[0] != '1')
            {
                return Json(HrSkillTypeItems, JsonRequestBehavior.AllowGet);
            }

            HrSkillTypeResponse resp = HrSkillTypeDAL.GetFilteredHrSkillTypes(Request);


            return Json(new { data = resp.HrSkillTypes, draw = Request["draw"], recordsTotal = resp.TotalRecordCount, recordsFiltered = resp.FilteredRecordCount }, JsonRequestBehavior.AllowGet);

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
                return Redirect(Url.Action("Index", "HrSkillType"));
            }
            HrSkillTypeInfo HrSkillType = HrSkillTypeDAL.GetSkillTypeByID(RouteData.Values["id"].ToString());
            if (HrSkillType == null)
            {
                return Redirect(Url.Action("Index", "HrSkillType"));
            }
            ViewBag.User = user;
            ViewBag.HrSkillTypes = HrSkillTypeDAL.GetAllHrSkillTypes();
            ViewBag.HrSkillType = HrSkillType;
            return View();
        }


        [HttpPost]
        public ActionResult Edit(HrSkillTypeInfo HrSkillType)
        {
            HrSkillTypeResponse response = new HrSkillTypeResponse();
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

            if (string.IsNullOrWhiteSpace(HrSkillType.SkillTypeName))
                response.ResponseMessages.Add("SkillType Name is required");
            else if (HrSkillTypeDAL.GetHrSkillTypeByName(HrSkillType.SkillTypeName, HrSkillType.RowId) != null)
                response.ResponseMessages.Add("SkillType Name already exists");

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";
                response.ResponseMessage = "<ul><li>" + response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(response);
            }
            try
            {
                HrSkillTypeDAL.UpdateHrSkillType(HrSkillType, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;
                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "SkillType is added successfully";
            response.HrSkillType = HrSkillType;
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
                return Redirect(Url.Action("Index", "HrSkillType"));
            }
            HrSkillTypeInfo HrSkillType = HrSkillTypeDAL.GetSkillTypeByID(RouteData.Values["id"].ToString());
            if (HrSkillType == null)
            {
                return Redirect(Url.Action("Index", "HrSkillType"));
            }
            ViewBag.User = user;
            ViewBag.HrSkillType = HrSkillType;
            return View();
        }

        [HttpPost]
        public ActionResult Delete(HrSkillTypeInfo HrSkillType)
        {
            HrSkillTypeResponse response = new HrSkillTypeResponse();
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
                HrSkillTypeDAL.DeleteHrSkillType(HrSkillType, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;
                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "SkillType is deleted successfully";
            response.HrSkillType = HrSkillType;
            return Json(response);
        } /**/
    }

}