using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrDepartmentGroup
{
    public class HrDepartmentGroupInfo
    {
        public string RowId { get; set; }

        public Int32 DepartmentGroupId { get; set; }

        public string DepartmentGroupName { get; set; }

        public Int32 Rank { get; set; }
    }
}