using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Employee.HrEmployeeEducation
{
    public class HrEmployeeEducationInfo
    {
        public string RowId { get; set; }
        public int EmployeeEducationId { get; set; }
        public string EmployeeId { get; set; }
        public string EducationalDegreeId { get; set; }
        public string EducationalInstituteId { get; set; }
        public string Session { get; set; }
        public string PassingYear { get; set; }
        public string Board { get; set; }
        public string Result { get; set; }
        public int Rank { get; set; }
        public string DegreeName { get; set; }
        public string InstituteName { get; set; }
    }
}