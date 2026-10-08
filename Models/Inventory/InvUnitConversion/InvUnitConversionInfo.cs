using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvUnitConversion
{
    public class InvUnitConversionInfo
    {
        //public string RowId { get; set; }

        //public int InvUnitConversionId { get; set; }

        //public string MinUnitName { get; set; }

        //public string FirstUnitId { get; set; }

        //public string SecondUnitId { get; set; }

        //public decimal ConversionRate { get; set; }

        //public string FirstUnitName { get; set; }
        //public string SecondUnitName { get; set; }

        public string RowId { get; set; }
        public string FirstUnitId { get; set; }
        public string SecondUnitId { get; set; }
        public decimal ConversionRate { get; set; }
    }
}