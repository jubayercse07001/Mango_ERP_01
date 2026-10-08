using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrGrade
{
    public class HrGradeInfo
    {
        public string RowId { get; set; }

        public Int32 GradeId { get; set; }

        public string GradeName { get; set; }

        public string PayScale { get; set; }

        public Int32 Rank { get; set; }

        public string EmployeeTypeId { get; set; }

        // Virtual Display Fields 
        public virtual string EmployeeTypeName { get; set; }
    }
}