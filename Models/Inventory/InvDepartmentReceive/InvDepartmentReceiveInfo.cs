using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvDepartmentReceive
{
    public class InvDepartmentReceiveInfo
    {
        public string RowId { get; set; }

        public int RequisitionDetailId { get; set; }

        public string RequisitionId { get; set; }

        public string ItemId { get; set; }

        public string UnitId { get; set; }

        public decimal Quantity { get; set; }

        public decimal ApprovedQuantity { get; set; }

        public decimal IssuedQuantity { get; set; }

        public decimal ReceivedQuantity { get; set; }

        public decimal UnitPriceEst { get; set; }
    }
}