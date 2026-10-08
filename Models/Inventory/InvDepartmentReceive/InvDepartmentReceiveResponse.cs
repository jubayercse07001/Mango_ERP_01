using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvDepartmentReceive
{
    public class InvDepartmentReceiveResponse : WebResponse
    {
        public InvDepartmentReceiveResponse()
        {
            ResponseTS = DateTime.Now;
            InvDepartmentReceives = new List<InvDepartmentReceiveInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public InvDepartmentReceiveInfo InvDepartmentReceive;
        public List<InvDepartmentReceiveInfo> InvDepartmentReceives;
    }
}
