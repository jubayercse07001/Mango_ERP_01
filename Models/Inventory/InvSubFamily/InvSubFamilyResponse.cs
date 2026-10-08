using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvSubFamily
{
    public class InvSubFamilyResponse : WebResponse
    {
        public InvSubFamilyResponse()
        {
            ResponseTS = DateTime.Now;
            InvSubFamilys = new List<InvSubFamilyInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public InvSubFamilyInfo InvSubFamily;
        public List<InvSubFamilyInfo> InvSubFamilys;
    }
}