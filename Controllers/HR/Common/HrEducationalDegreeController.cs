using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ERP.Models.HR.Common.HrEducationalDegree;
using ERP.Models.User;
using ERP.DAL.HR.Common;
using ERP.DAL;

namespace ERP.Controllers.HR.Common
{
    public class HrEducationalDegreeController : Controller
    {
        // GET: HrEducationalDegree
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
            ViewBag.HrEducationalDegrees = HrEducationalDegreeDAL.GetAllHrEducationalDegrees();
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
                return Redirect(Url.Action("Index", "HrEducationalDegree"));
            }
            ViewBag.User = user;
            ViewBag.HrEducationalDegree = HrEducationalDegreeDAL.GetAllHrEducationalDegrees();
            ViewBag.HrEducationalDegrees = HrEducationalDegreeDAL.GetAllHrEducationalDegrees();
            return View();
        }

        [HttpPost]
        public ActionResult Add(HrEducationalDegreeInfo HrEducationalDegree)
        {
            HrEducationalDegreeResponse response = new HrEducationalDegreeResponse();
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

            HrEducationalDegree.RowId = Guid.NewGuid().ToString();
            if (string.IsNullOrWhiteSpace(HrEducationalDegree.DegreeName))
                response.ResponseMessages.Add("Educational Degree Name is required");
            else if (HrEducationalDegreeDAL.GetHrEducationalDegreeByName(HrEducationalDegree.DegreeName, HrEducationalDegree.RowId) != null)
                response.ResponseMessages.Add("Educational Degree Name already exists");

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";
                response.ResponseMessage = "<ul><li>" + response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(response);
            }

            //HrEducationalDegree.RowId = Guid.NewGuid().ToString();
            try
            {
                HrEducationalDegreeDAL.AddHrEducationalDegree(HrEducationalDegree, lgUser);
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
            response.ResponseMessage = "Educational Degree is added successfully";
            response.HrEducationalDegree = HrEducationalDegree;
            return Json(response);
        }


        public ActionResult GetHrEducationalDegreeItemList()
        {
            List<HrEducationalDegreeInfo> HrEducationalDegreeItems = new List<HrEducationalDegreeInfo>();
            string lgUser = string.Empty;
            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();
            if (string.IsNullOrEmpty(lgUser))
            {
                return Json(HrEducationalDegreeItems, JsonRequestBehavior.AllowGet);
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null)
            {
                return Json(HrEducationalDegreeItems, JsonRequestBehavior.AllowGet);
            }
            if (user.PermissionGroup[0] != '1')
            {
                return Json(HrEducationalDegreeItems, JsonRequestBehavior.AllowGet);
            }

            HrEducationalDegreeResponse resp = HrEducationalDegreeDAL.GetFilteredHrEducationalDegrees(Request);


            return Json(new { data = resp.HrEducationalDegrees, draw = Request["draw"], recordsTotal = resp.TotalRecordCount, recordsFiltered = resp.FilteredRecordCount }, JsonRequestBehavior.AllowGet);

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
                return Redirect(Url.Action("Index", "HrEducationalDegree"));
            }
            HrEducationalDegreeInfo HrEducationalDegree = HrEducationalDegreeDAL.GetEducationalDegreeByID(RouteData.Values["id"].ToString());
            if (HrEducationalDegree == null)
            {
                return Redirect(Url.Action("Index", "HrEducationalDegree"));
            }
            ViewBag.User = user;
            ViewBag.HrEducationalDegrees = HrEducationalDegreeDAL.GetAllHrEducationalDegrees();
            ViewBag.HrEducationalDegree = HrEducationalDegree;
            return View();
        }


        [HttpPost]
        public ActionResult Edit(HrEducationalDegreeInfo HrEducationalDegree)
        {
            HrEducationalDegreeResponse response = new HrEducationalDegreeResponse();
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

            if (string.IsNullOrWhiteSpace(HrEducationalDegree.DegreeName))
                response.ResponseMessages.Add("Educational Degree Name is required");
            else if (HrEducationalDegreeDAL.GetHrEducationalDegreeByName(HrEducationalDegree.DegreeName, HrEducationalDegree.RowId) != null)
                response.ResponseMessages.Add("Educational Degree Name already exists");

            

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";
                response.ResponseMessage = "<ul><li>" + response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(response);
            }
            try
            {
                HrEducationalDegreeDAL.UpdateHrEducationalDegree(HrEducationalDegree, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;
                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "EducationalDegree is added successfully";
            response.HrEducationalDegree = HrEducationalDegree;
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
                return Redirect(Url.Action("Index", "HrEducationalDegree"));
            }
            HrEducationalDegreeInfo HrEducationalDegree = HrEducationalDegreeDAL.GetEducationalDegreeByID(RouteData.Values["id"].ToString());
            if (HrEducationalDegree == null)
            {
                return Redirect(Url.Action("Index", "HrEducationalDegree"));
            }
            ViewBag.User = user;
            ViewBag.HrEducationalDegree = HrEducationalDegree;
            return View();
        }

        [HttpPost]
        public ActionResult Delete(HrEducationalDegreeInfo HrEducationalDegree)
        {
            HrEducationalDegreeResponse response = new HrEducationalDegreeResponse();
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
                HrEducationalDegreeDAL.DeleteHrEducationalDegree(HrEducationalDegree, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;
                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "EducationalDegree is deleted successfully";
            response.HrEducationalDegree = HrEducationalDegree;
            return Json(response);
        } /**/
    }
}