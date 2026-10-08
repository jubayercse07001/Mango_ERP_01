using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Employee.HrEmployeeEducation
{
    public class HrEmployeeEducationResponse : WebResponse
    {
        public HrEmployeeEducationResponse()
        {
            ResponseTS = DateTime.Now;
            HrEmployeeEducations = new List<HrEmployeeEducationInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrEmployeeEducationInfo HrEmployeeEducation;
        public List<HrEmployeeEducationInfo> HrEmployeeEducations;
    }
}