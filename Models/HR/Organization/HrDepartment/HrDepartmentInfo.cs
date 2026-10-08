using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrDepartment
{
    public class HrDepartmentInfo
    {
        public string RowId { get; set; }

        public Int32 DepartmentId { get; set; }

        public string DepartmentName { get; set; }

        public string ShortDeptName { get; set; }

        // HrDepartmentGroup -> RowId
        public string DepartmentGroupId { get; set; }

        // HrBranch -> RowId
        public string BranchId { get; set; }

        // HrDivision -> RowId
        public string DivisionId { get; set; }

        public Int32 Rank { get; set; }

        // Virtual Fields
        public virtual string DepartmentGroupName { get; set; }

        public virtual string BranchName { get; set; }

        public virtual string DivisionName { get; set; }
        
    }
}