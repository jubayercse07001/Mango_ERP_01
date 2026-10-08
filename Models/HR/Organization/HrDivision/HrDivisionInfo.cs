using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrDivision
{
    public class HrDivisionInfo
    {
        public string RowId { get; set; }
        public Int32 DivisionId { get; set; }
        public string DivisionName { get; set; }
        public string ShortDivisionName { get; set; }
        public string Address { get; set; }

        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Mobile { get; set; }
        public string ParentId { get; set; } //CompanyInformation-> RowId
        public string MainDivisionId { get; set; }  // MainDivision-> RowId
        public Int32 Rank { get; set; }

        public virtual string CompanyName { get; set; }
        public virtual string MainDivisionName { get; set; }
    }
}