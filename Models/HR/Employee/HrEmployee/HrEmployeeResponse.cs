using Mango_ERP.Models.HR.Employee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Employee.HrEmployee
{
    public class HrEmployeeResponse : WebResponse
    {
        public HrEmployeeResponse()
        {
            ResponseTS = DateTime.Now;
            HrEmployees = new List<HrEmployeeInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrEmployeeInfo HrEmployee;
        public List<HrEmployeeInfo> HrEmployees;
    }
}