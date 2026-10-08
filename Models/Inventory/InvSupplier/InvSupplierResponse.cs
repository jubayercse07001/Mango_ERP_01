using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvSupplier
{
    public class InvSupplierResponse : WebResponse
    {
        public InvSupplierResponse()
        {
            ResponseTS = DateTime.Now;
            InvSuppliers = new List<InvSupplierInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public InvSupplierInfo InvSupplier;
        public List<InvSupplierInfo> InvSuppliers;
    }
}