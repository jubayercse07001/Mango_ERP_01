using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvDepartmentIssue
{
    public class InvDepartmentIssueResponse : WebResponse
    {
        public InvDepartmentIssueResponse()
        {
            ResponseTS = DateTime.Now;
            InvDepartmentIssues = new List<InvDepartmentIssueInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public InvDepartmentIssueInfo InvDepartmentIssue;
        public List<InvDepartmentIssueInfo> InvDepartmentIssues;
    }
}