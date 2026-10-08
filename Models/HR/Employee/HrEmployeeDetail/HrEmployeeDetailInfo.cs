using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Employee.EmployeeDetail
{
    public class HrEmployeeDetailInfo
    {
        public string RowId { get; set; }
        public int EmployeeDetailId { get; set; }
        public string EmployeeId { get; set; }
        public string FathersName { get; set; }
        public string MothersName { get; set; }
        public string NumberOfChildren { get; set; }
        public string SpouseName { get; set; }
        public string NationalIdNumber { get; set; }
        public string CityOfBirth { get; set; }
        public string DistrictIdOfBirth { get; set; }
        public string CountryIdOfBirth { get; set; }
        public string CountryIdOfCitizenship { get; set; }
        public string BirthMark { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }
        public string EmergencyContactPerson { get; set; }
        public string EmergenryContactNo { get; set; } 
        public string EmergenryContactRelation { get; set; } 
        public string TotalExperience { get; set; }
        public string WillingToTravel { get; set; }
        public string PassportNo { get; set; }
        public string SecurityClearance { get; set; }
        public string WorkAuthorizationId { get; set; }
        public string PreferedLocation { get; set; } 
        public string WillingToRelocate { get; set; }
        public string Remarks { get; set; }
        
    }
}