using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Common.HrJobType
{
    public class HrJobTypeResponse : WebResponse
    {
        public HrJobTypeResponse()
        {
            ResponseTS = DateTime.Now;
            HrJobTypes = new List<HrJobTypeInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrJobTypeInfo HrJobType;
        public List<HrJobTypeInfo> HrJobTypes;
    }
}