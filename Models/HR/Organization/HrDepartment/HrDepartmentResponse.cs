using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrDepartment
{
    public class HrDepartmentResponse : WebResponse
    {
        public HrDepartmentResponse()
        {
            ResponseTS = DateTime.Now;
            HrDepartments = new List<HrDepartmentInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrDepartmentInfo HrDepartment;
        public List<HrDepartmentInfo> HrDepartments;
    }
}