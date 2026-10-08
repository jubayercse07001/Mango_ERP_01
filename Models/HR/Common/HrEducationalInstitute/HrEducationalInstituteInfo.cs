using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Common.HrEducationalInstitute
{
    public class HrEducationalInstituteInfo
    {
            public string RowId { get; set; }
            public Int32 InstituteId { get; set; }
            public string InstituteName { get; set; }
            public string Address { get; set; }
            public Int32 Rank { get; set; }
         
    }
}