using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvGroup
{
    public class InvGroupResponse : WebResponse
    {
        public InvGroupResponse()
        {
            ResponseTS = DateTime.Now;
            InvGroups = new List<InvGroupInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public InvGroupInfo InvGroup;
        public List<InvGroupInfo> InvGroups;
    }
}