using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Common.HrDistrict
{
    public class HrDistrictResponse : WebResponse
    {
        public HrDistrictResponse()
        {
            ResponseTS = DateTime.Now;
            HrDistricts = new List<HrDistrictInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrDistrictInfo HrDistrict;
        public List<HrDistrictInfo> HrDistricts;
    }
}