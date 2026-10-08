using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.UnitConversion
{
    public class UnitConversionResponse : WebResponse
    {
        public UnitConversionResponse()
        {
            ResponseTS = DateTime.Now;
            UnitConversions = new List<UnitConversionInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public UnitConversionInfo UnitConversion;
        public List<UnitConversionInfo> UnitConversions;
    }
}