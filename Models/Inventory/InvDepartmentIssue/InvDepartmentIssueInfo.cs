using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvDepartmentIssue
{
    public class InvDepartmentIssueInfo
    {
        // ============================================================
        // REQUISITION DETAIL
        // ============================================================
        public string RowId { get; set; }          
        public decimal IssueQuantity { get; set; }
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public decimal ApprovedQuantity { get; set; }
        public decimal AlreadyIssuedQuantity { get; set; }
        public decimal RemainingQuantity { get; set; }

        public decimal Quantity { get; set; }

        public string RequestedUnitId { get; set; }

        public string RequestedUnitName { get; set; }

        public decimal UnitPriceEst { get; set; }

        public decimal TotalPriceEst { get; set; }

        public string Remarks { get; set; }
        public decimal IssuedQuantity { get; set; }
        // ============================================================
        // ISSUE INFORMATION
        // ============================================================

        public string IssueUnitId { get; set; }
        // ============================================================
        // UNIT CONVERSION
        // ============================================================
        public string UnitConversionId { get; set; }
        public decimal? ConversionRate { get; set; }
        public decimal? ConversionPrice { get; set; }
        public decimal? ConversionTotal { get; set; }
        public string IssueUnitName { get; set; }
    }
}