using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ERP.DAL;
using ERP.Models.HR.Employee.HrEmployee;
using ERP.Models.HR.Common.HrEducationalInstitute;
using ERP.Models.HR.Common.HrEducationalDegree;
using ERP.Models.User;
using Mango_ERP.Models;
using Mango_ERP.Models.HR.Employee;
using ERP.DAL.HR.Employee;
using ERP.DAL.HR.Organization;
using HrEmployeeResponse = ERP.DAL.HR.Employee.HrEmployeeResponse;
using ERP.DAL.HR.Common;
using ERP.Models;
using ERP.Models.HR.Employee.EmployeePicture;

namespace ERP.Controllers.HR.Organization
{
    public class HrEmployeeController : Controller
    {
        public object PictureFile { get; private set; }

        public ActionResult Index()
        {
            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Redirect(Url.Action("Logout", "Login"));

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Redirect(Url.Action("Logout", "Login"));

            if (user.PermissionGroup[0] != '1')
                return Redirect(Url.Action("Index", "Home"));

            ViewBag.HrEmployees = HrEmployeeDAL.GetAllHrEmployees();
            ViewBag.User = user;

            return View();
        }

        [HttpGet]
        public JsonResult GetDivisionByCompany(string CompanyId)
        {
            var divisionList =
                HrEmployeeDAL.GetDivisionByCompanyId(CompanyId);

            return Json(
                divisionList,
                JsonRequestBehavior.AllowGet
            );
        }
        [HttpGet]
        public JsonResult GetBranchByCompanyDivision(string CompanyId,string DivisionId)
        {
            var divisionList =
                HrEmployeeDAL.GetBranchByCompanyIdDivisionId(CompanyId,DivisionId);

            return Json(
                divisionList,
                JsonRequestBehavior.AllowGet
            );
        }

        [HttpGet]
        public JsonResult GetDepartmentByCompanyDivisionBranch(
    string CompanyId,
    string DivisionId,
    string BranchId)
        {
            var list = HrEmployeeDAL.GetDepartmentByCompanyIdDivisionIdBranchId(
                CompanyId,
                DivisionId,
                BranchId);

            return Json(list, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetDesignationByCompanyDivisionBranchDepartment(string CompanyId, string DivisionId, string BranchId, string DepartmentId)
        {
            var designationList = HrEmployeeDAL.GetDesignationByCompanyIdDivisionIdBranchIdDepartmentId(CompanyId, DivisionId, BranchId, DepartmentId);

            return Json(
                designationList,
                JsonRequestBehavior.AllowGet
            );
        }
        [HttpGet]
        public JsonResult GetEmployeeTypeByCompanyDivisionBranchDepartment(string CompanyId, string DivisionId, string BranchId, string DepartmentId)
        {
            var designationList = HrEmployeeDAL.GetEmployeeTypeByCompanyIdDivisionIdBranchIdDepartmentId(CompanyId, DivisionId, BranchId, DepartmentId);

            return Json(
                designationList,
                JsonRequestBehavior.AllowGet
            );
        }
        [HttpGet]
        public JsonResult GetStateByCountry(string countryId)
        {
            var StateList = HrStateDAL.GetStateByCountry(countryId);

            return Json(
                StateList,
                JsonRequestBehavior.AllowGet);
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
                return Redirect(Url.Action("Index", "HrEmployee"));

            ViewBag.User = user;
            ViewBag.CompanyList = HrCompanyInformationDAL.GetAllHrCompanyInformations();
            ViewBag.DivisionList = HrDivisionDAL.GetAllHrDivisions();
            ViewBag.BranchList = HrBranchDAL.GetAllHrBranches();
            ViewBag.DepartmentList = HrDepartmentDAL.GetAllHrDepartments();
            ViewBag.DesignationList = HrDesignationDAL.GetAllHrDesignations();
            ViewBag.EmployeeTypeList = HrEmployeeTypeDAL.GetAllHrEmployeeTypes();
            ViewBag.EngineerTypeList = HrEngineerTypeDAL.GetAllHrEngineerTypes();
            ViewBag.NationalityList = HrNationalityDAL.GetAllHrNationalitys();
            ViewBag.JobTypeList = HrJobTypeDAL.GetAllHrJobTypes();
            ViewBag.BloodGroupList = HrBloodGroupDAL.GetAllHrBloodGroups();
            ViewBag.ReligionList = HrReligionDAL.GetAllHrReligions();
            ViewBag.GradeList = HrGradeDAL.GetAllHrGrades();

            ViewBag.CountryList = HrCountryDAL.GetAllHrCountrys();
            ViewBag.DistrictList = HrDistrictDAL.GetAllHrDistrict();
            
            ViewBag.InstituteList = HrEducationalInstituteDAL.GetAllHrEducationalInstitutes();
            ViewBag.DegreeList = HrEducationalDegreeDAL.GetAllHrEducationalDegrees();
            ViewBag.SkillList = HrSkillDAL.GetAllHrSkills();


            return View();
        }

        [HttpPost]
        //public ActionResult Add(HrEmployeeInfo HrEmployee)
        public ActionResult Add(HrEmployeeInfo HrEmployee, HttpPostedFileBase PictureFile, HttpPostedFileBase SignatureFile)
        {
            // Renamed class type to prevent conflicting with the DataTables response class
            HrEmployeeActionResponse response = new HrEmployeeActionResponse();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                response.ResponseCode = "Not Logged In";
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

            if (string.IsNullOrWhiteSpace(HrEmployee.StaffCode))
                response.ResponseMessages.Add("Staff Code is required");
            else if (HrEmployeeDAL.GetEmployeeByStaffCode(HrEmployee.StaffCode, null) != null)
                response.ResponseMessages.Add("Staff Code already exists");

            if (string.IsNullOrWhiteSpace(HrEmployee.FirstName))
                response.ResponseMessages.Add("First Name is required");

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";
                response.ResponseMessage =
                    "<ul><li>" +
                    response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j)
                    + "</li></ul>";

                return Json(response);
            }

            // ===================================================================
            // CLEAR DATES IF CORRESPONDING CHECKBOXES ARE UNCHECKED
            // ===================================================================
            // Initialize EmployeePicture object if null
            if (HrEmployee.EmployeePicture == null)
            {
                HrEmployee.EmployeePicture = new EmployeePictureInfo();
            }

            // 1. Virtual and physical folder paths
            string employeePicFolderVirtual = "~/global_assets/images/employee/";
            string signatureFolderVirtual = "~/global_assets/images/signature/";

            string employeePicPhysicalDir = Server.MapPath(employeePicFolderVirtual);
            string signaturePhysicalDir = Server.MapPath(signatureFolderVirtual);

            // 2. Ensure directories exist on disk
            if (!System.IO.Directory.Exists(employeePicPhysicalDir))
            {
                System.IO.Directory.CreateDirectory(employeePicPhysicalDir);
            }
            if (!System.IO.Directory.Exists(signaturePhysicalDir))
            {
                System.IO.Directory.CreateDirectory(signaturePhysicalDir);
            }

            // 3. Process Profile Picture File
            if (PictureFile != null && PictureFile.ContentLength > 0)
            {
                string extension = System.IO.Path.GetExtension(PictureFile.FileName);
                string fileName = "Emp_" + (HrEmployee.StaffCode ?? Guid.NewGuid().ToString("N")) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                string physicalPath = System.IO.Path.Combine(employeePicPhysicalDir, fileName);

                PictureFile.SaveAs(physicalPath);
                HrEmployee.EmployeePicture.PicturePath = "/global_assets/images/employee/" + fileName;

                using (var reader = new System.IO.BinaryReader(PictureFile.InputStream))
                {
                    HrEmployee.EmployeePicture.Picture = reader.ReadBytes(PictureFile.ContentLength);
                }
            }

            // 4. Process Signature File
            if (SignatureFile != null && SignatureFile.ContentLength > 0)
            {
                string extension = System.IO.Path.GetExtension(SignatureFile.FileName);
                string fileName = "Sig_" + (HrEmployee.StaffCode ?? Guid.NewGuid().ToString("N")) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                string physicalPath = System.IO.Path.Combine(signaturePhysicalDir, fileName);

                SignatureFile.SaveAs(physicalPath);
                HrEmployee.EmployeePicture.SignaturePath = "/global_assets/images/signature/" + fileName;

                using (var reader = new System.IO.BinaryReader(SignatureFile.InputStream))
                {
                    HrEmployee.EmployeePicture.Signature = reader.ReadBytes(SignatureFile.ContentLength);
                }
            }
            try
            {
                HrEmployee.RowId = Guid.NewGuid().ToString();
                if (HrEmployee.EmployeeDetail != null)
                {
                    HrEmployee.EmployeeDetail.RowId = Guid.NewGuid().ToString();
                }
                HrEmployeeDAL.AddHrEmployee(HrEmployee, lgUser);
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
            response.ResponseMessage = "Employee added successfully";

            return Json(response);
        }

        public ActionResult GetHrEmployeeItemList()
        {
            string lgUser = Session["LoggedInUser"]?.ToString() ?? "";

            if (string.IsNullOrEmpty(lgUser))
                return Json(new List<HrEmployeeInfo>(), JsonRequestBehavior.AllowGet);

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);
            if (user == null)
                return Json(new List<HrEmployeeInfo>(), JsonRequestBehavior.AllowGet);

            // Correctly uses ERP.Models.HR.Employee.HrEmployee.HrEmployeeResponse
            HrEmployeeResponse resp = HrEmployeeDAL.GetFilteredHrEmployees(Request);

            return Json(new
            {
                data = resp.HrEmployees ?? new List<HrEmployeeInfo>(),
                draw = Request["draw"] ?? "1",
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
                return Redirect(Url.Action("Logout", "Login"));

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Redirect(Url.Action("Logout", "Login"));

            if (user.PermissionGroup[2] != '1')
                return Redirect(Url.Action("Index", "HrEmployee"));

            if (RouteData.Values["id"] == null)
                return Redirect(Url.Action("Index", "HrEmployee"));

            HrEmployeeInfo employee = HrEmployeeDAL.GetEmployeeByID(RouteData.Values["id"].ToString());

            if (employee == null)
                return Redirect(Url.Action("Index", "HrEmployee"));

            ViewBag.User = user;
            ViewBag.HrEmployee = employee;

            ViewBag.CompanyList = HrCompanyInformationDAL.GetAllHrCompanyInformations();
            ViewBag.DepartmentList = HrDepartmentDAL.GetAllHrDepartments();
            ViewBag.DesignationList = HrDesignationDAL.GetAllHrDesignations();
            ViewBag.DivisionList = HrDivisionDAL.GetAllHrDivisions();

            return View();
        }

        [HttpPost]
        public ActionResult Edit(HrEmployeeInfo HrEmployee)
        {
            HrEmployeeActionResponse response = new HrEmployeeActionResponse();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                return Json(response);
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
            {
                response.ResponseStatus = -1;
                return Json(response);
            }

            if (user.PermissionGroup[2] != '1')
            {
                response.ResponseStatus = -2;
                return Json(response);
            }

            if (string.IsNullOrWhiteSpace(HrEmployee.StaffCode))
                response.ResponseMessages.Add("Staff Code is required");
            else if (HrEmployeeDAL.GetEmployeeByStaffCode(HrEmployee.StaffCode, HrEmployee.RowId) != null)
                response.ResponseMessages.Add("Staff Code already exists for another record");

            if (string.IsNullOrWhiteSpace(HrEmployee.FirstName))
                response.ResponseMessages.Add("First Name is required");

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";

                response.ResponseMessage =
                    "<ul><li>" +
                    response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j)
                    + "</li></ul>";

                return Json(response);
            }

            try
            {
                HrEmployeeDAL.UpdateHrEmployee(HrEmployee, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;

                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "Employee updated successfully";

            return Json(response);
        }

        [HttpGet]
        public ActionResult Delete()
        {
            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Redirect(Url.Action("Logout", "Login"));

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Redirect(Url.Action("Logout", "Login"));

            if (user.PermissionGroup[3] != '1')
                return Redirect(Url.Action("Index", "HrEmployee"));

            if (RouteData.Values["id"] == null)
                return Redirect(Url.Action("Index", "HrEmployee"));

            HrEmployeeInfo employee = HrEmployeeDAL.GetEmployeeByID(RouteData.Values["id"].ToString());

            if (employee == null)
                return Redirect(Url.Action("Index", "HrEmployee"));

            ViewBag.User = user;
            ViewBag.HrEmployee = employee;

            return View();
        }

        [HttpPost]
        public ActionResult Delete(HrEmployeeInfo HrEmployee)
        {
            HrEmployeeActionResponse response = new HrEmployeeActionResponse();
            string lgUser = Session["LoggedInUser"]?.ToString();

            try
            {
                HrEmployeeDAL.DeleteHrEmployee(HrEmployee.RowId, lgUser);

                response.ResponseStatus = 1;
                response.ResponseMessage = "Employee deleted successfully";
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseMessage = ex.Message;
            }

            return Json(response);
        }
    }

    // Renamed this class to avoid name collision with DataTables response class
    public class HrEmployeeActionResponse
    {
        public int ResponseStatus { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public List<string> ResponseMessages { get; set; } = new List<string>();
    }
}