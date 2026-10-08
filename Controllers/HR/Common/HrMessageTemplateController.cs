using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ERP.Models.HR.Common.HrMessageTemplate;
using ERP.Models.User;
using ERP.DAL.HR.Common;
using ERP.DAL;

namespace ERP.Controllers.HR.Common
{

    public class HrMessageTemplateController : Controller
    {
        // GET: HrMessageTemplate

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
            ViewBag.HrMessageTemplates = HrMessageTemplateDAL.GetAllHrMessageTemplates();
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
                return Redirect(Url.Action("Index", "HrMessageTemplate"));
            }
            ViewBag.User = user;
            ViewBag.HrMessageTemplate = HrMessageTemplateDAL.GetAllHrMessageTemplates();
            ViewBag.HrMessageTemplates = HrMessageTemplateDAL.GetAllHrMessageTemplates();
            return View();
        }

        [HttpPost]
        public ActionResult Add(HrMessageTemplateInfo HrMessageTemplate)
        {
            HrMessageTemplateResponse response = new HrMessageTemplateResponse();
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

            HrMessageTemplate.RowId = Guid.NewGuid().ToString();
            if (string.IsNullOrWhiteSpace(HrMessageTemplate.TemplateName))
                response.ResponseMessages.Add("Template Name is required");
            else if (HrMessageTemplateDAL.GetHrMessageTemplateByName(HrMessageTemplate.TemplateName, HrMessageTemplate.RowId) != null)
                response.ResponseMessages.Add("Template Name already exists");
            if (string.IsNullOrWhiteSpace(HrMessageTemplate.Subject))
                response.ResponseMessages.Add("Subject is required");
            if (string.IsNullOrWhiteSpace(HrMessageTemplate.Body))
                response.ResponseMessages.Add("Body is required");
            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";
                response.ResponseMessage = "<ul><li>" + response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(response);
            }

            HrMessageTemplate.RowId = Guid.NewGuid().ToString();
            try
            {
                HrMessageTemplateDAL.AddHrMessageTemplate(HrMessageTemplate, lgUser);
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
            response.ResponseMessage = "Template is added successfully";
            response.HrMessageTemplate = HrMessageTemplate;
            return Json(response);
        }


        public ActionResult GetHrMessageTemplateItemList()
        {
            List<HrMessageTemplateInfo> HrMessageTemplateItems = new List<HrMessageTemplateInfo>();
            string lgUser = string.Empty;
            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();
            if (string.IsNullOrEmpty(lgUser))
            {
                return Json(HrMessageTemplateItems, JsonRequestBehavior.AllowGet);
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null)
            {
                return Json(HrMessageTemplateItems, JsonRequestBehavior.AllowGet);
            }
            if (user.PermissionGroup[0] != '1')
            {
                return Json(HrMessageTemplateItems, JsonRequestBehavior.AllowGet);
            }

            HrMessageTemplateResponse resp = HrMessageTemplateDAL.GetFilteredHrMessageTemplates(Request);


            return Json(new { data = resp.HrMessageTemplates, draw = Request["draw"], recordsTotal = resp.TotalRecordCount, recordsFiltered = resp.FilteredRecordCount }, JsonRequestBehavior.AllowGet);

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
                return Redirect(Url.Action("Index", "HrMessageTemplate"));
            }
            HrMessageTemplateInfo HrMessageTemplate = HrMessageTemplateDAL.GetTemplateByID(RouteData.Values["id"].ToString());
            if (HrMessageTemplate == null)
            {
                return Redirect(Url.Action("Index", "HrMessageTemplate"));
            }
            ViewBag.User = user;
            ViewBag.HrMessageTemplates = HrMessageTemplateDAL.GetAllHrMessageTemplates();
            ViewBag.HrMessageTemplate = HrMessageTemplate;
            return View();
        }


        [HttpPost]
        public ActionResult Edit(HrMessageTemplateInfo HrMessageTemplate)
        {
            HrMessageTemplateResponse response = new HrMessageTemplateResponse();
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

            if (string.IsNullOrWhiteSpace(HrMessageTemplate.TemplateName))
                response.ResponseMessages.Add("Template Name is required");
            else if (HrMessageTemplateDAL.GetHrMessageTemplateByName(HrMessageTemplate.TemplateName, HrMessageTemplate.RowId) != null)
                response.ResponseMessages.Add("Template Name already exists");

            if (string.IsNullOrWhiteSpace(HrMessageTemplate.Subject))
                response.ResponseMessages.Add("Subject is required");
            if (string.IsNullOrWhiteSpace(HrMessageTemplate.Body))
                response.ResponseMessages.Add("Body is required");
            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";
                response.ResponseMessage = "<ul><li>" + response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(response);
            }
            try
            {
                HrMessageTemplateDAL.UpdateHrMessageTemplate(HrMessageTemplate, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;
                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "Template is added successfully";
            response.HrMessageTemplate = HrMessageTemplate;
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
                return Redirect(Url.Action("Index", "HrMessageTemplate"));
            }
            HrMessageTemplateInfo HrMessageTemplate = HrMessageTemplateDAL.GetTemplateByID(RouteData.Values["id"].ToString());
            if (HrMessageTemplate == null)
            {
                return Redirect(Url.Action("Index", "HrMessageTemplate"));
            }
            ViewBag.User = user;
            ViewBag.HrMessageTemplate = HrMessageTemplate;
            return View();
        }

        [HttpPost]
        public ActionResult Delete(HrMessageTemplateInfo HrMessageTemplate)
        {
            HrMessageTemplateResponse response = new HrMessageTemplateResponse();
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
                HrMessageTemplateDAL.DeleteHrMessageTemplate(HrMessageTemplate, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;
                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "Template is deleted successfully";
            response.HrMessageTemplate = HrMessageTemplate;
            return Json(response);
        } /**/
    }

}