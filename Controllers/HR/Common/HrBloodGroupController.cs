using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ERP.Models.HR.Common.HrBloodGroup;
using ERP.Models.User;
using ERP.DAL.HR.Common;
using ERP.DAL;

namespace ERP.Controllers.HR.Common
{
   
        public class HrBloodGroupController : Controller
        {
            // GET: HrBloodGroup

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
                ViewBag.HrBloodGroups = HrBloodGroupDAL.GetAllHrBloodGroups();
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
                            return Redirect(Url.Action("Index", "HrBloodGroup"));
                        }
                        ViewBag.User = user;
                        ViewBag.HrBloodGroup = HrBloodGroupDAL.GetAllHrBloodGroups();
                        ViewBag.HrBloodGroups = HrBloodGroupDAL.GetAllHrBloodGroups();
                        return View();
                    }

                    [HttpPost]
                    public ActionResult Add(HrBloodGroupInfo HrBloodGroup)
                    {
                        HrBloodGroupResponse response = new HrBloodGroupResponse();
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

                        //if (string.IsNullOrEmpty(HrBloodGroup.HrBloodGroupId))
                        //{
                        //    response.ResponseMessages.Add("Blood Group is required");
                        //}

                        
                        if (string.IsNullOrEmpty(HrBloodGroup.BloodGroupName))
                        {
                            response.ResponseMessages.Add("Blood Group Name is required");
                        }

                        if (!string.IsNullOrEmpty(HrBloodGroup.BloodGroupName))
                        {
                            if (HrBloodGroupDAL.GetHrBloodGroupByName(HrBloodGroup.BloodGroupName) != null)
                            {
                                response.ResponseMessages.Add("Blood Group Name  already exists");
                            }
                        }

                        if (response.ResponseMessages.Count > 0)
                        {
                            response.ResponseStatus = 0;
                            response.ResponseCode = "Invalid Data";
                            response.ResponseMessage = "<ul><li>" + response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                            return Json(response);
                        }

                        HrBloodGroup.RowId = Guid.NewGuid().ToString();
                        try
                        {
                            HrBloodGroupDAL.AddHrBloodGroup(HrBloodGroup, lgUser);
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
                        response.ResponseMessage = "Bank Account is added successfully";
                        response.HrBloodGroup = HrBloodGroup;
                        return Json(response);
                    }  


            public ActionResult GetHrBloodGroupItemList()
            {
                List<HrBloodGroupInfo> HrBloodGroupItems = new List<HrBloodGroupInfo>();
                string lgUser = string.Empty;
                if (Session["LoggedInUser"] != null)
                    lgUser = Session["LoggedInUser"].ToString();
                if (string.IsNullOrEmpty(lgUser))
                {
                    return Json(HrBloodGroupItems, JsonRequestBehavior.AllowGet);
                }

                UserInfo user = UserDAL.GetUserByUserCode(lgUser);
                if (user == null)
                {
                    return Json(HrBloodGroupItems, JsonRequestBehavior.AllowGet);
                }
                if (user.PermissionGroup[0] != '1')
                {
                    return Json(HrBloodGroupItems, JsonRequestBehavior.AllowGet);
                }

                HrBloodGroupResponse resp = HrBloodGroupDAL.GetFilteredHrBloodGroups(Request);


                return Json(new { data = resp.HrBloodGroups, draw = Request["draw"], recordsTotal = resp.TotalRecordCount, recordsFiltered = resp.FilteredRecordCount }, JsonRequestBehavior.AllowGet);

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
                    return Redirect(Url.Action("Index", "HrBloodGroup"));
                }
                HrBloodGroupInfo HrBloodGroup = HrBloodGroupDAL.GetBloodGroupByID(RouteData.Values["id"].ToString());
                if (HrBloodGroup == null)
                {
                    return Redirect(Url.Action("Index", "HrBloodGroup"));
                }
                ViewBag.User = user;
                ViewBag.HrBloodGroups = HrBloodGroupDAL.GetAllHrBloodGroups();
                ViewBag.HrBloodGroup = HrBloodGroup;
                return View();
            }


            [HttpPost]
            public ActionResult Edit(HrBloodGroupInfo HrBloodGroup)
            {
                HrBloodGroupResponse response = new HrBloodGroupResponse();
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

                if (string.IsNullOrEmpty(HrBloodGroup.BloodGroupName))
                {
                    response.ResponseMessages.Add("Blood Group Name is required");
                }
                else if (HrBloodGroupDAL.GetHrBloodGroupByName(HrBloodGroup.BloodGroupName) != null)
                {
                    response.ResponseMessages.Add("Blood Group Name  already exists");
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
                    HrBloodGroupDAL.UpdateHrBloodGroup(HrBloodGroup, lgUser);
                }
                catch (Exception ex)
                {
                    response.ResponseStatus = 0;
                    response.ResponseCode = "Database Error";
                    response.ResponseMessage = ex.Message;
                    return Json(response);
                }

                response.ResponseStatus = 1;
                response.ResponseMessage = "Blood Group is added successfully";
                response.HrBloodGroup = HrBloodGroup;
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
                    return Redirect(Url.Action("Index", "HrBloodGroup"));
                }
                HrBloodGroupInfo HrBloodGroup = HrBloodGroupDAL.GetBloodGroupByID(RouteData.Values["id"].ToString());
                if (HrBloodGroup == null)
                {
                    return Redirect(Url.Action("Index", "HrBloodGroup"));
                }
                ViewBag.User = user;
                ViewBag.HrBloodGroup = HrBloodGroup;
                return View();
            }

            [HttpPost]
            public ActionResult Delete(HrBloodGroupInfo HrBloodGroup)
            {
                HrBloodGroupResponse response = new HrBloodGroupResponse();
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
                    HrBloodGroupDAL.DeleteHrBloodGroup(HrBloodGroup, lgUser);
                }
                catch (Exception ex)
                {
                    response.ResponseStatus = 0;
                    response.ResponseCode = "Database Error";
                    response.ResponseMessage = ex.Message;
                    return Json(response);
                }

                response.ResponseStatus = 1;
                response.ResponseMessage = "Bank Account is deleted successfully";
                response.HrBloodGroup = HrBloodGroup;
                return Json(response);
            } /**/
        }
    
}