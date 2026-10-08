using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Common.HrReligion
{
    public class HrReligionInfo
    {
        public string RowId { get; set; }
        public Int32 ReligionId { get; set; }
        public string ReligionName { get; set; }
        public Int32 Rank { get; set; }
    }
}