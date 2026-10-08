using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrDesignationGroup
{
    public class HrDesignationGroupResponse : WebResponse
    {
        public HrDesignationGroupResponse()
        {
            ResponseTS = DateTime.Now;
            HrDesignationGroups = new List<HrDesignationGroupInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrDesignationGroupInfo HrDesignationGroup;
        public List<HrDesignationGroupInfo> HrDesignationGroups;
    }
}