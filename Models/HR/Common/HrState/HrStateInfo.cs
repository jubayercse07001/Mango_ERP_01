using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Common.HrState
{
    public class HrStateInfo
    {
        public string RowId { get; set; }

        public string CountryId { get; set; }
        public Int32 StateId { get; set; }
        public string StateName { get; set; }
        public string StateCode { get; set; }


        public virtual string CountryName { get; set; }
    }
}