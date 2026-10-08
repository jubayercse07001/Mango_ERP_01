using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrCompanyInformation
{
    public class HrCompanyInformationResponse : WebResponse
    {
        public HrCompanyInformationResponse()
        {
            ResponseTS = DateTime.Now;
            HrCompanyInformations = new List<HrCompanyInformationInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrCompanyInformationInfo HrCompanyInformation;
        public List<HrCompanyInformationInfo> HrCompanyInformations;
    }
}