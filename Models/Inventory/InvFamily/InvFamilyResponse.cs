using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvFamily
{
    public class InvFamilyResponse : WebResponse
    {
        public InvFamilyResponse()
        {
            ResponseTS = DateTime.Now;
            InvFamilys = new List<InvFamilyInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public InvFamilyInfo InvFamily;
        public List<InvFamilyInfo> InvFamilys;
    }
}