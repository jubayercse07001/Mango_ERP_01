using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvProperty
{
    public class InvPropertyResponse : WebResponse
    {
        public InvPropertyResponse()
        {
            ResponseTS = DateTime.Now;
            InvPropertys = new List<InvPropertyInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public InvPropertyInfo InvProperty;
        public List<InvPropertyInfo> InvPropertys;
    }
}