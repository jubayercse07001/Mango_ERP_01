using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Common.HrEngineerType
{
    public class HrEngineerTypeResponse : WebResponse
    {
        public HrEngineerTypeResponse()
        {
            ResponseTS = DateTime.Now;
            HrEngineerTypes = new List<HrEngineerTypeInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrEngineerTypeInfo HrEngineerType;
        public List<HrEngineerTypeInfo> HrEngineerTypes;
    }
}