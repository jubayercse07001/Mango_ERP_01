using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Common.HrReligion
{
    public class HrReligionResponse : WebResponse
    {
        public HrReligionResponse()
        {
            ResponseTS = DateTime.Now;
            HrReligions = new List<HrReligionInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrReligionInfo HrReligion;
        public List<HrReligionInfo> HrReligions;
    }
}