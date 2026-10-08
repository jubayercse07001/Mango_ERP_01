using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Common.HrBenefit
{
    public class HrBenefitResponse : WebResponse
    {
        public HrBenefitResponse()
        {
            ResponseTS = DateTime.Now;
            HrBenefits = new List<HrBenefitInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrBenefitInfo HrBenefit;
        public List<HrBenefitInfo> HrBenefits;
    }
}