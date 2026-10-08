using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvUnit
{
    public class InvUnitInfo
    {
        public string RowId { get; set; }
        public int UnitId { get; set; }
        public string MinUnitName { get; set; }
        public string UnitSymbol { get; set; }
    }
}