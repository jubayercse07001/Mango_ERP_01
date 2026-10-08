using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvDepartment
{
    public class InvDepartmentResponse : WebResponse
    {
        public InvDepartmentResponse()
        {
            ResponseTS = DateTime.Now;
            InvDepartments = new List<InvDepartmentInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public InvDepartmentInfo InvDepartment;
        public List<InvDepartmentInfo> InvDepartments;
    }
}