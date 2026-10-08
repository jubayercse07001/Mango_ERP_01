using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrSection
{
    public class HrSectionResponse : WebResponse
    {
        public HrSectionResponse()
        {
            ResponseTS = DateTime.Now;
            HrSections = new List<HrSectionInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrSectionInfo HrSection;
        public List<HrSectionInfo> HrSections;
    }
}