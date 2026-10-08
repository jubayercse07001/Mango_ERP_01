using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvUnit
{
    public class InvUnitResponse : WebResponse
    {
        public InvUnitResponse()
        {
            ResponseTS = DateTime.Now;
            InvUnits = new List<InvUnitInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public InvUnitInfo InvUnit;
        public List<InvUnitInfo> InvUnits;
    }
}