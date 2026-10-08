using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvUnitConversion
{
    public class InvUnitConversionResponse : WebResponse
    {
        public InvUnitConversionResponse()
        {
            ResponseTS = DateTime.Now;
            InvUnitConversions = new List<InvUnitConversionInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public InvUnitConversionInfo InvUnitConversion;
        public List<InvUnitConversionInfo> InvUnitConversions;
    }
}