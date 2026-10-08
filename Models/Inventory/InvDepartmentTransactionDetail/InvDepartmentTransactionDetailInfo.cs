using System;

namespace ERP.Models.Inventory.InvDepartmentTransactionDetail
{
    public class InvDepartmentTransactionDetailInfo
    {
        public string RowId { get; set; }

        public int TransactionDetailId { get; set; }

        public string TransactionId { get; set; }

        public string ItemId { get; set; }

        public string UnitId { get; set; }

        public decimal Quantity { get; set; }

        public decimal MRRQty { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }

        public string UnitConversionId { get; set; }

        public decimal? ConversionRate { get; set; }

        public decimal? ConversionPrice { get; set; }

        public decimal? ConversionTotal { get; set; }

        public int DomainStatus { get; set; }

        public string CreateUser { get; set; }

        public string UpdateUser { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? UpdateDate { get; set; }

        // ---- Display-only (joined) ----
        public virtual string ItemName { get; set; }

        public virtual string UnitName { get; set; }
    }
}
