using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ERP.Models.HR.Organization.HrDesignationGroup;
using ERP.Models.User;
using ERP.DAL.HR.Organization;
using ERP.DAL;

namespace ERP.Controllers.HR.Common
{

    public class HrDesignationGroupController : Controller
    {
        // GET: HrDesignationGroup

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
            ViewBag.HrDesignationGroups = HrDesignationGroupDAL.GetAllHrDesignationGroups();
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
                return Redirect(Url.Action("Index", "HrDesignationGroup"));
            }
            ViewBag.User = user;
            //ViewBag.HrDesignationGroup = HrDesignationGroupDAL.GetAllHrDesignationGroups();
            //ViewBag.HrDesignationGroups = HrDesignationGroupDAL.GetAllHrDesignationGroups();
            ViewBag.HrDesignationGroups = HrDesignationGroupDAL.GetAllHrDesignationGroups();
            return View();
        }

        [HttpPost]
        public ActionResult Add(HrDesignationGroupInfo HrDesignationGroup)
        {
            HrDesignationGroupResponse response = new HrDesignationGroupResponse();
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

            HrDesignationGroup.RowId = Guid.NewGuid().ToString();
            if (string.IsNullOrWhiteSpace(HrDesignationGroup.DesignationGroup))
                response.ResponseMessages.Add("Designation Group is required");
            else if (HrDesignationGroupDAL.GetHrDesignationGroupByName(HrDesignationGroup.DesignationGroup, HrDesignationGroup.RowId) != null)
                response.ResponseMessages.Add("Designation Group already exists");
            else if (HrDesignationGroup.Rank <= 0)
            {
                response.ResponseMessages.Add("Rank is required");
            }
            
            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";
                response.ResponseMessage = "<ul><li>" + response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(response);
            }

            HrDesignationGroup.RowId = Guid.NewGuid().ToString();
            try
            {
                HrDesignationGroupDAL.AddHrDesignationGroup(HrDesignationGroup, lgUser);
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
            response.ResponseMessage = "DesignationGroup is added successfully";
            response.HrDesignationGroup = HrDesignationGroup;
            return Json(response);
        }


        public ActionResult GetHrDesignationGroupItemList()
        {
            List<HrDesignationGroupInfo> HrDesignationGroupItems = new List<HrDesignationGroupInfo>();
            string lgUser = string.Empty;
            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();
            if (string.IsNullOrEmpty(lgUser))
            {
                return Json(HrDesignationGroupItems, JsonRequestBehavior.AllowGet);
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null)
            {
                return Json(HrDesignationGroupItems, JsonRequestBehavior.AllowGet);
            }
            if (user.PermissionGroup[0] != '1')
            {
                return Json(HrDesignationGroupItems, JsonRequestBehavior.AllowGet);
            }

            HrDesignationGroupResponse resp = HrDesignationGroupDAL.GetFilteredHrDesignationGroups(Request);


            return Json(new { data = resp.HrDesignationGroups, draw = Request["draw"], recordsTotal = resp.TotalRecordCount, recordsFiltered = resp.FilteredRecordCount }, JsonRequestBehavior.AllowGet);

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
                return Redirect(Url.Action("Index", "HrDesignationGroup"));
            }
            HrDesignationGroupInfo HrDesignationGroup = HrDesignationGroupDAL.GetDesignationGroupByID(RouteData.Values["id"].ToString());
            if (HrDesignationGroup == null)
            {
                return Redirect(Url.Action("Index", "HrDesignationGroup"));
            }
            ViewBag.User = user;
            //ViewBag.HrDesignationGroups = HrDesignationGroupDAL.GetAllHrDesignationGroups();
            ViewBag.HrDesignationGroup = HrDesignationGroup;
            ViewBag.HrDesignationGroups = HrDesignationGroupDAL.GetAllHrDesignationGroups();
            return View();
        }


        [HttpPost]
        public ActionResult Edit(HrDesignationGroupInfo HrDesignationGroup)
        {
            HrDesignationGroupResponse response = new HrDesignationGroupResponse();
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

            if (string.IsNullOrWhiteSpace(HrDesignationGroup.DesignationGroup))
                response.ResponseMessages.Add("Designation Group is required");
            else if (HrDesignationGroupDAL.GetHrDesignationGroupByName(HrDesignationGroup.DesignationGroup, HrDesignationGroup.RowId) != null)
                response.ResponseMessages.Add("Designation Group already exists");

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";
                response.ResponseMessage = "<ul><li>" + response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(response);
            }
            try
            {
                HrDesignationGroupDAL.UpdateHrDesignationGroup(HrDesignationGroup, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;
                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "DesignationGroup is added successfully";
            response.HrDesignationGroup = HrDesignationGroup;
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
                return Redirect(Url.Action("Index", "HrDesignationGroup"));
            }
            HrDesignationGroupInfo HrDesignationGroup = HrDesignationGroupDAL.GetDesignationGroupByID(RouteData.Values["id"].ToString());
            if (HrDesignationGroup == null)
            {
                return Redirect(Url.Action("Index", "HrDesignationGroup"));
            }
            ViewBag.User = user;
            ViewBag.HrDesignationGroup = HrDesignationGroup;
            return View();
        }

        [HttpPost]
        public ActionResult Delete(HrDesignationGroupInfo HrDesignationGroup)
        {
            HrDesignationGroupResponse response = new HrDesignationGroupResponse();
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
                HrDesignationGroupDAL.DeleteHrDesignationGroup(HrDesignationGroup, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;
                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "DesignationGroup is deleted successfully";
            response.HrDesignationGroup = HrDesignationGroup;
            return Json(response);
        } /**/
    }

}