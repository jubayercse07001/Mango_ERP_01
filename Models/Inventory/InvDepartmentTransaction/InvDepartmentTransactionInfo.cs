using System;
using System.Collections.Generic;

using ERP.Models.Inventory.InvDepartmentTransactionDetail;

namespace ERP.Models.Inventory.InvDepartmentTransaction
{
    public class InvDepartmentTransactionInfo
    {
        public string RowId { get; set; }

        public int TransactionId { get; set; }

        public string TransactionCode { get; set; }

        public DateTime TransactionDate { get; set; }

        public string RequisitionId { get; set; }

        public string MaterialReveiveId { get; set; }

        public string MaterialIssueId { get; set; }

        public string TransactionType { get; set; }

        public string FromDepartment { get; set; }

        public string ToDepartment { get; set; }

        public decimal TotalAmount { get; set; }

        public string CurrencyId { get; set; }

        public string MIVNo { get; set; }

        public string INCMIVNo { get; set; }

        public string FinancialYearId { get; set; }

        public string Remarks { get; set; }

        public int? IsIssued { get; set; }

        public int? IsReceived { get; set; }

        public int? IsRemoved { get; set; }

        public virtual string RequisitionCode { get; set; }

        public virtual string RequisitionName { get; set; }

        public virtual string FromDepartmentName { get; set; }

        public virtual string ToDepartmentName { get; set; }

        public virtual string CurrencyName { get; set; }

        public virtual string MaterialReceiveNo { get; set; }

        public virtual string MaterialIssueNo { get; set; }

        // Populated on Add/Edit postback — the detail grid rows
        public virtual List<InvDepartmentTransactionDetailInfo> Details { get; set; }

        public class InvDepartmentTransactionResponse
        {
            public int ResponseStatus { get; set; }
            public string ResponseCode { get; set; }
            public string ResponseMessage { get; set; }
            public List<string> ResponseMessages { get; set; } = new List<string>();
        }
    }
}
