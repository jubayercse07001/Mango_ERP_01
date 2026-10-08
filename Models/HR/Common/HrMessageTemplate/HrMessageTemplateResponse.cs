using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Common.HrMessageTemplate
{
    public class HrMessageTemplateResponse : WebResponse
    {
        public HrMessageTemplateResponse()
        {
            ResponseTS = DateTime.Now;
            HrMessageTemplates = new List<HrMessageTemplateInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrMessageTemplateInfo HrMessageTemplate;
        public List<HrMessageTemplateInfo> HrMessageTemplates;
    }
}