using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrBranch
{
    public class HrBranchResponse : WebResponse
    {
        public HrBranchResponse()
        {
            ResponseTS = DateTime.Now;
            HrBranchs = new List<HrBranchInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public List<HrBranchInfo> HrBranches { get; internal set; }

        public HrBranchInfo HrBranch;
        public List<HrBranchInfo> HrBranchs;
    }
}