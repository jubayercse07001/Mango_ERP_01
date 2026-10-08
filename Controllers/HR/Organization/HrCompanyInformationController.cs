using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ERP.Models.HR.Organization.HrCompanyInformation;
using ERP.Models.User;
using ERP.DAL.HR.Organization;
using ERP.DAL;

namespace ERP.Controllers.HR.Common
{

    public class HrCompanyInformationController : Controller
    {
        // GET: HrCompanyInformation

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
            ViewBag.HrCompanyInformations = HrCompanyInformationDAL.GetAllHrCompanyInformations();
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
                return Redirect(Url.Action("Index", "HrCompanyInformation"));
            }
            ViewBag.User = user;
            //ViewBag.HrCompanyInformation = HrCompanyInformationDAL.GetAllHrCompanyInformations();
            //ViewBag.HrCompanyInformations = HrCompanyInformationDAL.GetAllHrCompanyInformations();
            ViewBag.HrCompanyInformations = HrCompanyInformationDAL.GetAllHrCompanyInformations();
            return View();
        }

        [HttpPost]
        public ActionResult Add(HrCompanyInformationInfo HrCompanyInformation)
        {
            HrCompanyInformationResponse response = new HrCompanyInformationResponse();
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

            HrCompanyInformation.RowId = Guid.NewGuid().ToString();
            if (string.IsNullOrWhiteSpace(HrCompanyInformation.CompanyName))
                response.ResponseMessages.Add("Company Name is required");
            else if (HrCompanyInformationDAL.GetHrCompanyInformationByName(HrCompanyInformation.CompanyName, HrCompanyInformation.RowId) != null)
                response.ResponseMessages.Add("Company Name already exists");
            if (HrCompanyInformation.LogoFile != null &&
                    HrCompanyInformation.LogoFile.ContentLength > 0)
            {
                using (var binaryReader =
                    new System.IO.BinaryReader(HrCompanyInformation.LogoFile.InputStream))
                {
                    HrCompanyInformation.Logo =
                        binaryReader.ReadBytes(HrCompanyInformation.LogoFile.ContentLength);
                }
            }

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";
                response.ResponseMessage = "<ul><li>" + response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(response);
            }

            HrCompanyInformation.RowId = Guid.NewGuid().ToString();
            try
            {
                HrCompanyInformationDAL.AddHrCompanyInformation(HrCompanyInformation, lgUser);
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
            response.HrCompanyInformation = HrCompanyInformation;
            return Json(response);
        }


        public ActionResult GetHrCompanyInformationItemList()
        {
            List<HrCompanyInformationInfo> HrCompanyInformationItems = new List<HrCompanyInformationInfo>();
            string lgUser = string.Empty;
            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();
            if (string.IsNullOrEmpty(lgUser))
            {
                return Json(HrCompanyInformationItems, JsonRequestBehavior.AllowGet);
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null)
            {
                return Json(HrCompanyInformationItems, JsonRequestBehavior.AllowGet);
            }
            if (user.PermissionGroup[0] != '1')
            {
                return Json(HrCompanyInformationItems, JsonRequestBehavior.AllowGet);
            }

            HrCompanyInformationResponse resp = HrCompanyInformationDAL.GetFilteredHrCompanyInformations(Request);


            return Json(new { data = resp.HrCompanyInformations, draw = Request["draw"], recordsTotal = resp.TotalRecordCount, recordsFiltered = resp.FilteredRecordCount }, JsonRequestBehavior.AllowGet);

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
                return Redirect(Url.Action("Index", "HrCompanyInformation"));
            }
            HrCompanyInformationInfo HrCompanyInformation = HrCompanyInformationDAL.GetCompanyInformationByID(RouteData.Values["id"].ToString());
            if (HrCompanyInformation == null)
            {
                return Redirect(Url.Action("Index", "HrCompanyInformation"));
            }
            ViewBag.User = user;
            //ViewBag.HrCompanyInformations = HrCompanyInformationDAL.GetAllHrCompanyInformations();
            ViewBag.HrCompanyInformation = HrCompanyInformation;
            ViewBag.HrCompanyInformations = HrCompanyInformationDAL.GetAllHrCompanyInformations();
            return View();
        }


        [HttpPost]
        public ActionResult Edit(HrCompanyInformationInfo HrCompanyInformation)
        {
            HrCompanyInformationResponse response = new HrCompanyInformationResponse();
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

            if (string.IsNullOrWhiteSpace(HrCompanyInformation.CompanyName))
                response.ResponseMessages.Add("Company Name is required");
            else if (HrCompanyInformationDAL.GetHrCompanyInformationByName(HrCompanyInformation.CompanyName, HrCompanyInformation.RowId) != null)
                response.ResponseMessages.Add("Company Name already exists");

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";
                response.ResponseMessage = "<ul><li>" + response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(response);
            }
            try
            {
                HrCompanyInformationDAL.UpdateHrCompanyInformation(HrCompanyInformation, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;
                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "Company Information is added successfully";
            response.HrCompanyInformation = HrCompanyInformation;
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
                return Redirect(Url.Action("Index", "HrCompanyInformation"));
            }
            HrCompanyInformationInfo HrCompanyInformation = HrCompanyInformationDAL.GetCompanyInformationByID(RouteData.Values["id"].ToString());
            if (HrCompanyInformation == null)
            {
                return Redirect(Url.Action("Index", "HrCompanyInformation"));
            }
            ViewBag.User = user;
            ViewBag.HrCompanyInformation = HrCompanyInformation;
            return View();
        }

        [HttpPost]
        public ActionResult Delete(HrCompanyInformationInfo HrCompanyInformation)
        {
            HrCompanyInformationResponse response = new HrCompanyInformationResponse();
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
                HrCompanyInformationDAL.DeleteHrCompanyInformation(HrCompanyInformation, lgUser);
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
            response.HrCompanyInformation = HrCompanyInformation;
            return Json(response);
        } /**/
    }

}