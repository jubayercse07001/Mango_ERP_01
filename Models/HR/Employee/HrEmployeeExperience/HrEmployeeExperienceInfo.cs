using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mango_ERP.Models
{
    public class HrEmployeeExperienceInfo
    {
        public string RowId { get; set; }
        public int EmployeeExperienceId { get; set; }
        public string EmployeeId { get; set; }

        public string CompanyName { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public string CompanyLocation { get; set; }

        public string CompanyAddress { get; set; }
        public string CompanyContactNo { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Web { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool IsCurrentlyWorking { get; set; }

        public string Duration { get; set; } // Stored / calculated duration string (e.g., "2 yrs 3 mos")
        public string Responsibilities { get; set; }
        public int Rank { get; set; }

    }
}