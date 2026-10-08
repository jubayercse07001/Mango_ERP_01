using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Common.HrMessageTemplate
{
    public class HrMessageTemplateInfo
    {
        public string RowId { get; set; }
        public Int32 TemplateId { get; set; }
        public string TemplateName { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}