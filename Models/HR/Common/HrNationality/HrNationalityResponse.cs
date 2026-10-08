using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Common.HrNationality
{
    public class HrNationalityResponse : WebResponse
    {
        public HrNationalityResponse()
        {
            ResponseTS = DateTime.Now;
            HrNationalitys = new List<HrNationalityInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrNationalityInfo HrNationality;
        public List<HrNationalityInfo> HrNationalitys;
    }
}