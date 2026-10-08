using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Common.HrEducationalInstitute
{
    public class HrEducationalInstituteResponse : WebResponse
    {
        public HrEducationalInstituteResponse()
        {
            ResponseTS = DateTime.Now;
            HrEducationalInstitutes = new List<HrEducationalInstituteInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrEducationalInstituteInfo HrEducationalInstitute;
        public List<HrEducationalInstituteInfo> HrEducationalInstitutes;
    }
}