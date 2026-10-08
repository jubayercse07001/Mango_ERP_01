using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ERP.Models.Inventory.InvDepartmentRequisitionDetail;

namespace ERP.Models.Inventory.InvDepartmentRequisition
{
    public class InvDepartmentRequisitionInfo
    {
        public string RowId { get; set; }

        public int RequisitionId { get; set; }

        public DateTime RequisitionDate { get; set; }

        public string RequisitionCode { get; set; }

        public string RequisitionName { get; set; }

        public string DepartmentId { get; set; }

        public string RequestedBy { get; set; }

        public string JobId { get; set; }

        public string JobNo { get; set; }

        public string RequiredFor { get; set; }

        public int Status { get; set; }

        public int IsApproved { get; set; }

        public int IsIssued { get; set; }
        public int IsReceived { get; set; }

        public int IsRemoved { get; set; }
        

        // ---- Optional display properties (joined / derived, not stored columns) ----
        public virtual string DepartmentName { get; set; }

        //public virtual string RequestedByName { get; set; }

        public virtual string RequestedByName { get; set; }

        public virtual string JobName { get; set; }

        public virtual string StatusText { get; set; }

        public virtual string ApprovedText { get; set; }

        public virtual string IssuedText { get; set; }
        public virtual string LastName { get; set; }
        

        // Populated on Add/Edit postback — the detail grid rows (Item lines)
        public virtual List<InvDepartmentRequisitionDetailInfo> Details { get; set; }

        public class InvDepartmentRequisitionResponse
        {
            public int ResponseStatus { get; set; }
            public string ResponseCode { get; set; }
            public string ResponseMessage { get; set; }
            public List<string> ResponseMessages { get; set; } = new List<string>();
        }
    }
}
