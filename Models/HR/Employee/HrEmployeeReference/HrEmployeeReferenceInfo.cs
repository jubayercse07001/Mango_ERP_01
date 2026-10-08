using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace ERP.Models.HR.Employee.HrEmployeeReference
{
    public class HrEmployeeReferenceInfo
    {
        public string RowId { get; set; }
        public int EmployeeReferenceId { get; set; }
        public string EmployeeId { get; set; }
        public string ReferenceName { get; set; }
        public string Occupation { get; set; }
        public string Designation { get; set; }
        public string Company { get; set; }
        public string Relation { get; set; }
        public int ContactNo { get; set; }
        public string PresentAddress { get; set; }
        public string PermanentAddress { get; set; }
        public string Email { get; set; }
        public string NId { get; set; }
        public string Web { get; set; }

        
    }
}