using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvDepartmentRequisitionDetail
{
    public class InvDepartmentRequisitionDetailInfo
    {
        public string RowId { get; set; }

        public int RequisitionDetailId { get; set; }

        public string RequisitionId { get; set; }

        public string ItemId { get; set; }

        public decimal Quantity { get; set; }

        public string UnitId { get; set; }
        public decimal ApprovedQuantity { get; set; }

        public decimal UnitPriceEst { get; set; }
        public decimal IssuedQuantity { get; set; }
        public string UnitName { get; set; }
        public decimal TotalPriceEst { get; set; }
        public string Remarks { get; set; }
        public virtual string ItemName { get; set; }

        public virtual string MinUnitName { get; set; }
        
        public decimal ReceivedQuantity { get; set; }

        public decimal ConversionRate { get; set; }
        public decimal ConversionPrice { get; set; }
        public decimal ConversionTotal { get; set; }
        public string UnitConversionId { get; set; }


    }
}
