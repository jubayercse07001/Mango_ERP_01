using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvManufacturer
{
    public class InvManufacturerInfo
    {
        public string RowId { get; set; }
        public int ManufacturerId { get; set; }
        public string ManufacturerName { get; set; }
        public string ManufacturerAddress { get; set; }
    }
}