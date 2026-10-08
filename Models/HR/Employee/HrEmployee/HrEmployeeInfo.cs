using ERP.Models.HR.Employee.EmployeeDetail;
using ERP.Models.HR.Employee.HrEmployeeEducation;
using ERP.Models.HR.Employee.HrEmployeeReference;
using ERP.Models.HR.Employee.HrEmployeeTraining;
using ERP.Models.HR.Employee.HrEmployeeProfessionalKnowledge;
using ERP.Models.HR.Employee.EmployeePicture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mango_ERP.Models.HR.Employee
{
    public class HrEmployeeInfo
    {
        public string RowId { get; set; }
        public int EmployeeId { get; set; }
        public string CompanyId { get; set; }
        public string DivisionId { get; set; }
        public string BranchId { get; set; }
        public string DepartmentId { get; set; }
        public string SectionId { get; set; }
        public string DesignationId { get; set; }
        public string EmployeeTypeId { get; set; }
        public string EngineerTypeId { get; set; }
        public string JobType { get; set; }

        public string StaffCode { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string PresentAddress { get; set; }
        public string PermanentAddress { get; set; }
        public string BloodGroupId { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string NationalityId { get; set; }
        public int NationalIdNumber { get; set; }
        public int HomePhone { get; set; }
        public int CellPhone { get; set; }
        public int OfficePhone { get; set; }
        public string PrimaryEmail { get; set; }
        public string AlternateEmail { get; set; }
        public string Gender { get; set; }
        public string ReligionId { get; set; }
        public string MaritalStatus { get; set; }
        public DateTime? DateOfJoin { get; set; }
        public DateTime? DateOfRetirement { get; set; }
        public DateTime? DateOfPermanent { get; set; }
        public string EmployeeDetailId { get; set; }
        public string EmployeeEducationId { get; set; }
        public string EmployeeExperienceId { get; set; }
        public string TrainingId { get; set; }
        public string ProfessionalKnowledgeId { get; set; }
        public string EmployeeReferenceId { get; set; }
        public string HrEmployeePictureId { get; set; }
        
        public bool IsSalaried { get; set; }
        public double BasicSalary { get; set; }
        public double CurrentSalary { get; set; }
        public bool IsConfirmed { get; set; }
        public DateTime? ConfirmationDate { get; set; }
        public bool IsTerminated { get; set; }
        public DateTime? TerminateDate { get; set; }
        public string TerminateBy { get; set; }
        public string TerminateReason { get; set; }
        public bool IsInLPR { get; set; }
        public string GradeId { get; set; }
        public int BankAccountNumber { get; set; }
        public string UserId { get; set; }
        
        public bool IsPoliceVerified { get; set; }
        public string PoliceVerificationNumber { get; set; }
        public string PeferredJob { get; set; }
       
        
        public bool IsAdminEmployee { get; set; }
        public bool IsEngineer { get; set; }
        public int Rank { get; set; }

        public bool IsPermanent { get; set; }
        public bool IsRetired { get; set; }
        public DateTime? TerminatedDate { get; set; }

        public virtual string CompanyName { get; set; }
        public virtual string BranchName { get; set; }
        public virtual string MainDivisionName { get; set; }
        public virtual string DivisionName { get; set; }
        public virtual string DepartmentName { get; set; }
        public virtual string SectionName { get; set; }
        public virtual string DesignationGroup { get; set; }
        public virtual string DesignationName { get; set; }
        public virtual string EmployeeTypeName { get; set; }
        public virtual string EngineerTypeName { get; set; }
        public virtual string NationalityName { get; set; }
        public virtual string GradeName { get; set; }

        public virtual string DisctrictName { get; set; }

        public virtual string DegreeName { get; set; }
        public virtual string InstituteName { get; set; }

        public virtual string TrainingCategoryName { get; set; }
        public virtual string TrainingTitle { get; set; }
        public virtual string SkillTypeName { get; set; }

        public HrEmployeeDetailInfo EmployeeDetail { get; set; }

        public List<HrEmployeeEducationInfo> Educations { get; set; }

        public List<HrEmployeeExperienceInfo> Experiences { get; set; }

        public List<HrEmployeeTrainingInfo> Trainings { get; set; }

        public List<HrEmployeeProfessionalKnowledgeInfo> ProfessionalKnowledges { get; set; }

        public List<HrEmployeeReferenceInfo> References { get; set; }
        public EmployeePictureInfo EmployeePicture { get; set; } 

    }
}