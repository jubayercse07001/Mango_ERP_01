using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvClass
{
    public class InvClassResponse : WebResponse
    {
        public InvClassResponse()
        {
            ResponseTS = DateTime.Now;
            InvClasss = new List<InvClassInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public InvClassInfo InvClass;
        public List<InvClassInfo> InvClasss;
    }
}