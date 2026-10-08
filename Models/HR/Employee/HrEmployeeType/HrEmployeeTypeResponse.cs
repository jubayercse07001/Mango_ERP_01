using ERP.Models.HR.Organization.HrEmployeeType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Employee
{
    public class HrEmployeeTypeResponse : WebResponse
    {
        public HrEmployeeTypeResponse()
        {
            ResponseTS = DateTime.Now;
            HrEmployeeTypes = new List<HrEmployeeTypeInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrEmployeeTypeInfo HrEmployeeType;
        public List<HrEmployeeTypeInfo> HrEmployeeTypes;
    }
}