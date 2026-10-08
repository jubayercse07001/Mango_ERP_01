using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrMainDivision
{
    public class HrMainDivisionInfo
    {
        public string RowId { get; set; }
        public Int32 MainDivisionId { get; set; }
        public string MainDivisionName { get; set; }
        public string ShortMainDivisionName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public Int32 Rank { get; set; }
    }
}