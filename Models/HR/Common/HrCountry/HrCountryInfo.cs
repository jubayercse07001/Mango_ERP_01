using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Common.HrCountry
{
    public class HrCountryInfo
    {
        public string RowId { get; set; }
        public Int32 CountryId { get; set; }
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
    }
}