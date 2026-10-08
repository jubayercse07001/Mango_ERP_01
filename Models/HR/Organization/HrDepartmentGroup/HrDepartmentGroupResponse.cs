using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrDepartmentGroup
{
    public class HrDepartmentGroupResponse : WebResponse
    {
        public HrDepartmentGroupResponse()
        {
            ResponseTS = DateTime.Now;
            HrDepartmentGroups = new List<HrDepartmentGroupInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrDepartmentGroupInfo HrDepartmentGroup;
        public List<HrDepartmentGroupInfo> HrDepartmentGroups;
    }
}