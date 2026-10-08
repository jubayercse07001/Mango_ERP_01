using Mango_ERP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Employee.HrEmployeeExperience
{
    public class HrEmployeeExperienceResponse : WebResponse
    {
        public HrEmployeeExperienceResponse()
        {
            ResponseTS = DateTime.Now;
            HrEmployeeExperiences = new List<HrEmployeeExperienceInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrEmployeeExperienceInfo HrEmployeeExperience;
        public List<HrEmployeeExperienceInfo> HrEmployeeExperiences;
    }
}