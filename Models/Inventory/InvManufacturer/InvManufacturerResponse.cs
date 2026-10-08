using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvManufacturer
{
    public class InvManufacturerResponse : WebResponse
    {
        public InvManufacturerResponse()
        {
            ResponseTS = DateTime.Now;
            InvManufacturers = new List<InvManufacturerInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public InvManufacturerInfo InvManufacturer;
        public List<InvManufacturerInfo> InvManufacturers;
    }
}