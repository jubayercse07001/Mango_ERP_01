using System;

namespace ERP.Models.Inventory.UnitConversion
{
    public class UnitConversionInfo
    {
        public string FromUnitId { get; set; }
        public string ToUnitId { get; set; }

        public string RowId { get; set; }
        public string FirstUnitId { get; set; }
        public string SecondUnitId { get; set; }
        public decimal ConversionRate { get; set; }
    }
}