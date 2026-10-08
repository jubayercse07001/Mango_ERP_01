using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ERP.Models.HR.Common.HrEducationalInstitute;
using ERP.Models.User;
using ERP.DAL.HR.Common;
using ERP.DAL;

namespace ERP.Controllers.HR.Common
{
    public class HrEducationalInstituteController : Controller
    {
        // GET: HrEducationalInstitute
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
            ViewBag.HrEducationalInstitutes = HrEducationalInstituteDAL.GetAllHrEducationalInstitutes();
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
                return Redirect(Url.Action("Index", "HrEducationalInstitute"));
            }
            ViewBag.User = user;
            ViewBag.HrEducationalInstitute = HrEducationalInstituteDAL.GetAllHrEducationalInstitutes();
            ViewBag.HrEducationalInstitutes = HrEducationalInstituteDAL.GetAllHrEducationalInstitutes();
            return View();
        }

        [HttpPost]
        public ActionResult Add(HrEducationalInstituteInfo HrEducationalInstitute)
        {
            HrEducationalInstituteResponse response = new HrEducationalInstituteResponse();
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

            HrEducationalInstitute.RowId = Guid.NewGuid().ToString();
            if (string.IsNullOrWhiteSpace(HrEducationalInstitute.InstituteName))
                response.ResponseMessages.Add("Educational Institute Name is required");
            else if (HrEducationalInstituteDAL.GetHrEducationalInstituteByName(HrEducationalInstitute.InstituteName, HrEducationalInstitute.RowId) != null)
                response.ResponseMessages.Add("Educational Institute Name already exists");

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";
                response.ResponseMessage = "<ul><li>" + response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(response);
            }

            //HrEducationalInstitute.RowId = Guid.NewGuid().ToString();
            try
            {
                HrEducationalInstituteDAL.AddHrEducationalInstitute(HrEducationalInstitute, lgUser);
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
            response.ResponseMessage = "Educational Institute is added successfully";
            response.HrEducationalInstitute = HrEducationalInstitute;
            return Json(response);
        }


        public ActionResult GetHrEducationalInstituteItemList()
        {
            List<HrEducationalInstituteInfo> HrEducationalInstituteItems = new List<HrEducationalInstituteInfo>();
            string lgUser = string.Empty;
            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();
            if (string.IsNullOrEmpty(lgUser))
            {
                return Json(HrEducationalInstituteItems, JsonRequestBehavior.AllowGet);
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null)
            {
                return Json(HrEducationalInstituteItems, JsonRequestBehavior.AllowGet);
            }
            if (user.PermissionGroup[0] != '1')
            {
                return Json(HrEducationalInstituteItems, JsonRequestBehavior.AllowGet);
            }

            HrEducationalInstituteResponse resp = HrEducationalInstituteDAL.GetFilteredHrEducationalInstitutes(Request);


            return Json(new { data = resp.HrEducationalInstitutes, draw = Request["draw"], recordsTotal = resp.TotalRecordCount, recordsFiltered = resp.FilteredRecordCount }, JsonRequestBehavior.AllowGet);

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
                return Redirect(Url.Action("Index", "HrEducationalInstitute"));
            }
            HrEducationalInstituteInfo HrEducationalInstitute = HrEducationalInstituteDAL.GetEducationalInstituteByID(RouteData.Values["id"].ToString());
            if (HrEducationalInstitute == null)
            {
                return Redirect(Url.Action("Index", "HrEducationalInstitute"));
            }
            ViewBag.User = user;
            ViewBag.HrEducationalInstitutes = HrEducationalInstituteDAL.GetAllHrEducationalInstitutes();
            ViewBag.HrEducationalInstitute = HrEducationalInstitute;
            return View();
        }


        [HttpPost]
        public ActionResult Edit(HrEducationalInstituteInfo HrEducationalInstitute)
        {
            HrEducationalInstituteResponse response = new HrEducationalInstituteResponse();
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

            if (string.IsNullOrWhiteSpace(HrEducationalInstitute.InstituteName))
                response.ResponseMessages.Add("Educational Institute Name is required");
            else if (HrEducationalInstituteDAL.GetHrEducationalInstituteByName(HrEducationalInstitute.InstituteName, HrEducationalInstitute.RowId) != null)
                response.ResponseMessages.Add("Educational Institute Name already exists");



            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";
                response.ResponseMessage = "<ul><li>" + response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(response);
            }
            try
            {
                HrEducationalInstituteDAL.UpdateHrEducationalInstitute(HrEducationalInstitute, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;
                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "EducationalInstitute is added successfully";
            response.HrEducationalInstitute = HrEducationalInstitute;
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
                return Redirect(Url.Action("Index", "HrEducationalInstitute"));
            }
            HrEducationalInstituteInfo HrEducationalInstitute = HrEducationalInstituteDAL.GetEducationalInstituteByID(RouteData.Values["id"].ToString());
            if (HrEducationalInstitute == null)
            {
                return Redirect(Url.Action("Index", "HrEducationalInstitute"));
            }
            ViewBag.User = user;
            ViewBag.HrEducationalInstitute = HrEducationalInstitute;
            return View();
        }

        [HttpPost]
        public ActionResult Delete(HrEducationalInstituteInfo HrEducationalInstitute)
        {
            HrEducationalInstituteResponse response = new HrEducationalInstituteResponse();
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
                HrEducationalInstituteDAL.DeleteHrEducationalInstitute(HrEducationalInstitute, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;
                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "EducationalInstitute is deleted successfully";
            response.HrEducationalInstitute = HrEducationalInstitute;
            return Json(response);
        } /**/
    }
}