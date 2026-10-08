using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using ERP.DAL;
using ERP.DAL.HR.Common;

using ERP.Models.User;
using ERP.Models.HR.Organization.HrApprovedPosition;
using ERP.DAL.HR.Organization;

namespace ERP.Controllers.HR.Organization
{
    public class HrApprovedPositionController : Controller
    {
        public ActionResult Index()
        {
            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Redirect(Url.Action("Logout", "Login"));

            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Redirect(Url.Action("Logout", "Login"));

            if (user.PermissionGroup[0] != '1')
                return Redirect(Url.Action("Index", "Home"));

            ViewBag.User = user;

            return View();
        }
        [HttpGet]
        public JsonResult GetDivisionByCompany(string CompanyId)
        {
            var divisionList =
                HrApprovedPositionDAL.GetDivisionByCompanyId(CompanyId);

            return Json(
                divisionList,
                JsonRequestBehavior.AllowGet
            );
        }
        [HttpGet]
        public JsonResult GetBranchByDivision(string DivisionId)
        {
            var divisionList =
                HrApprovedPositionDAL.GetGetBranchByDivisionId(DivisionId);

            return Json(
                divisionList,
                JsonRequestBehavior.AllowGet
            );
        }
        
        [HttpGet]
        public JsonResult GetDepartmentByBranch(string BranchId)
        {
            var divisionList =
                HrApprovedPositionDAL.GetDepartmentByBranchId(BranchId);

            return Json(
                divisionList,
                JsonRequestBehavior.AllowGet
            );
        }
        
        [HttpGet]
        public JsonResult GetDesignationByGroup(string DesignationGroupId)
        {
            var designationList = HrApprovedPositionDAL.GetDesignationByGroupId(DesignationGroupId);

            return Json(
                designationList,
                JsonRequestBehavior.AllowGet
            );
        }
        [HttpGet]
        public JsonResult GetNextPositionRank(string companyId, string branchId, string departmentId)
        {
            int nextRank = HrApprovedPositionDAL.GetNextPositionRank(companyId, branchId, departmentId);
            return Json(nextRank, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult Add()
        {
            string lgUser = "";
            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Redirect(Url.Action("Logout", "Login"));

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null)
                return Redirect(Url.Action("Logout", "Login"));

            if (user.PermissionGroup[1] != '1')
                return Redirect(Url.Action("Index", "HrApprovedPosition"));

            ViewBag.User = user;

            // --- FIX: Ensure you fetch the configuration object and populate the ViewBag ---
            // Make sure GetAllPosition() exists and returns a valid object or fallback structure
            var position = HrApprovedPositionDAL.GetAllPosition();
            ViewBag.PositionMaster = position ?? new HrApprovedPositionInfo { NextPositionRank = 1 };

            ViewBag.CompanyList = HrCompanyInformationDAL.GetAllHrCompanyInformations();
            ViewBag.BranchList = HrBranchDAL.GetAllHrBranches();
            ViewBag.MainDivisionList = HrMainDivisionDAL.GetAllHrMainDivisions();
            ViewBag.DivisionList = HrDivisionDAL.GetAllHrDivisions();
            ViewBag.DepartmentList = HrDepartmentDAL.GetAllHrDepartments();
            ViewBag.EmployeeTypeList = HrEmployeeTypeDAL.GetAllHrEmployeeTypes();
            ViewBag.DesignationGroupList = HrDesignationGroupDAL.GetAllHrDesignationGroups();
            ViewBag.DesignationList = HrDesignationDAL.GetAllHrDesignations();

            return View();
        }

        [HttpPost]
        public ActionResult Add(
            HrApprovedPositionInfo HrApprovedPosition)
        {
            HrApprovedPositionResponse response =
                new HrApprovedPositionResponse();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                response.ResponseCode = "Not Logged In";

                return Json(response);
            }

            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

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

            if (string.IsNullOrEmpty(
                HrApprovedPosition.DesignationId))
            {
                response.ResponseMessages
                    .Add("Designation is required");
            }

            if (HrApprovedPosition.ApprovedPost <= 0)
            {
                response.ResponseMessages
                    .Add("Approved Post is required");
            }

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";

                response.ResponseMessage =
                    "<ul><li>" +
                    response.ResponseMessages.Aggregate(
                        (i, j) => i + "</li><li>" + j)
                    + "</li></ul>";

                return Json(response);
            }

            try
            {
                HrApprovedPosition.RowId =
                    Guid.NewGuid().ToString();

                HrApprovedPositionDAL.AddHrApprovedPosition(
                    HrApprovedPosition,
                    lgUser);
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
            response.ResponseMessage =
                "Approved Position added successfully";

            return Json(response);
        }

        public ActionResult GetHrApprovedPositionItemList()
        {
            List<HrApprovedPositionInfo> list =
                new List<HrApprovedPositionInfo>();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Json(list,
                    JsonRequestBehavior.AllowGet);

            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Json(list,
                    JsonRequestBehavior.AllowGet);

            HrApprovedPositionResponse resp =
                HrApprovedPositionDAL
                .GetFilteredHrApprovedPositions(Request);

            return Json(new
            {
                data = resp.HrApprovedPositions,
                draw = Request["draw"],
                recordsTotal = resp.TotalRecordCount,
                recordsFiltered = resp.FilteredRecordCount
            },
            JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Edit()
        {
            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Redirect(
                    Url.Action("Logout", "Login"));

            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Redirect(
                    Url.Action("Logout", "Login"));

            if (user.PermissionGroup[2] != '1')
                return Redirect(
                    Url.Action("Index",
                    "HrApprovedPosition"));

            HrApprovedPositionInfo position =
                HrApprovedPositionDAL
                .GetApprovedPositionByID(
                RouteData.Values["id"].ToString());

            if (position == null)
                return Redirect(
                    Url.Action("Index",
                    "HrApprovedPosition"));

            ViewBag.User = user;
            ViewBag.HrApprovedPosition = position;

            ViewBag.CompanyList =
                HrCompanyInformationDAL.GetAllHrCompanyInformations();

            ViewBag.BranchList =
                HrBranchDAL.GetAllHrBranches();

            ViewBag.MainDivisionList =
                HrMainDivisionDAL.GetAllHrMainDivisions();

            ViewBag.DivisionList =
                HrDivisionDAL.GetAllHrDivisions();

            ViewBag.DepartmentList =
                HrDepartmentDAL.GetAllHrDepartments();

            //ViewBag.SectionList = HrSectionDAL.GetAllHrSections();

            ViewBag.EmployeeTypeList =
                HrEmployeeTypeDAL.GetAllHrEmployeeTypes();

            ViewBag.DesignationGroupList =
                HrDesignationGroupDAL.GetAllHrDesignationGroups();

            ViewBag.DesignationList =
                HrDesignationDAL.GetAllHrDesignations();

            return View();
        }

        [HttpPost]
        public ActionResult Edit(
            HrApprovedPositionInfo HrApprovedPosition)
        {
            HrApprovedPositionResponse response =
                new HrApprovedPositionResponse();

            string lgUser =
                Session["LoggedInUser"]?.ToString();

            try
            {
                HrApprovedPositionDAL
                    .UpdateHrApprovedPosition(
                        HrApprovedPosition,
                        lgUser);

                response.ResponseStatus = 1;
                response.ResponseMessage =
                    "Approved Position updated successfully";
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseMessage =
                    ex.Message;
            }

            return Json(response);
        }

        [HttpGet]
        public ActionResult Delete()
        {
            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Redirect(
                    Url.Action("Logout", "Login"));

            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Redirect(
                    Url.Action("Logout", "Login"));

            if (user.PermissionGroup[3] != '1')
                return Redirect(
                    Url.Action("Index",
                    "HrApprovedPosition"));

            HrApprovedPositionInfo position =
                HrApprovedPositionDAL
                .GetApprovedPositionByID(
                RouteData.Values["id"].ToString());

            if (position == null)
                return Redirect(
                    Url.Action("Index",
                    "HrApprovedPosition"));

            ViewBag.User = user;
            ViewBag.HrApprovedPosition = position;

            return View();
        }

        [HttpPost]
        public ActionResult Delete(
            HrApprovedPositionInfo HrApprovedPosition)
        {
            HrApprovedPositionResponse response =
                new HrApprovedPositionResponse();

            string lgUser =
                Session["LoggedInUser"]?.ToString();

            try
            {
                HrApprovedPositionDAL
                    .DeleteHrApprovedPosition(
                        HrApprovedPosition,
                        lgUser);

                response.ResponseStatus = 1;
                response.ResponseMessage =
                    "Approved Position deleted successfully";
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseMessage =
                    ex.Message;
            }

            return Json(response);
        }
    }
}