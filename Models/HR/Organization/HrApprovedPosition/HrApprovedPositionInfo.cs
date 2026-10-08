using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrApprovedPosition
{
    public class HrApprovedPositionInfo
    {
        
        public string RowId { get; set; }
        public int PositionId { get; set; } 
        public int ApprovedPost { get; set; }
        public int PositionRank { get; set; }
        public string CompanyId { get; set; }
        public string BranchId { get; set; }
        public string MainDivisionId { get; set; }
        public string DivisionId { get; set; }
        public string DepartmentId { get; set; }
        public string SectionId { get; set; }
        public string EmployeeTypeId { get; set; }
        public string DesignationGroupId { get; set; }
        public string DesignationId { get; set; }
        public virtual string CompanyName { get; set; }
        public virtual string BranchName { get; set; }
        public virtual string MainDivisionName { get; set; }
        public virtual string DivisionName { get; set; }
        public virtual string DepartmentName { get; set; }
        public virtual string SectionName { get; set; }
        public virtual string DesignationGroup { get; set; }
        public virtual string DesignationName { get; set; }
        public virtual string EmployeeTypeName { get; set; }
        public virtual int NextPositionRank { get; set; }
    }
}