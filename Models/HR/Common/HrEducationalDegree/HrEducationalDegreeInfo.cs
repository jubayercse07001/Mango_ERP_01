using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Common.HrEducationalDegree
{
    public class HrEducationalDegreeInfo
    {
        public string RowId { get; set; }
        public Int32 DegreeId { get; set; }
        public string DegreeName { get; set; }
        public Int32 Rank { get; set; }
    }
}