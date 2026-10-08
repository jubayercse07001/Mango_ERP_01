using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrBranch
{
    public class HrBranchInfo
    {
        public string RowId { get; set; }

        public Int32 BranchId { get; set; }

        // HrDivision -> RowId
        public string DivisionId { get; set; }

        public string BranchName { get; set; }

        public string ShortBranchName { get; set; }

        public string BranchAddress { get; set; }

        public string ContactNumber { get; set; }

        public Int32 Rank { get; set; }

        // Virtual Fields
        public virtual string DivisionName { get; set; }

    }

}