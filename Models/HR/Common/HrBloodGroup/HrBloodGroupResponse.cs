using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Common.HrBloodGroup
{
    public class HrBloodGroupResponse : WebResponse
    {
        public HrBloodGroupResponse()
        {
            ResponseTS = DateTime.Now;
            HrBloodGroups = new List<HrBloodGroupInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrBloodGroupInfo HrBloodGroup;
        public List<HrBloodGroupInfo> HrBloodGroups;
    }
}