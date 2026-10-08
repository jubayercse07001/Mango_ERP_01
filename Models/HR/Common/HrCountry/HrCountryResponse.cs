using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Common.HrCountry
{
    public class HrCountryResponse : WebResponse
    {
        public HrCountryResponse()
        {
            ResponseTS = DateTime.Now;
            HrCountrys = new List<HrCountryInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrCountryInfo HrCountry;
        public List<HrCountryInfo> HrCountrys;
    }
}