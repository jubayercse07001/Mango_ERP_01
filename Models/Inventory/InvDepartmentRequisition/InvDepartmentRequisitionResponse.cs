using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvDepartmentRequisition
{
    public class InvDepartmentRequisitionResponse : WebResponse
    {
        public InvDepartmentRequisitionResponse()
        {
            ResponseTS = DateTime.Now;
            InvDepartmentRequisitions = new List<InvDepartmentRequisitionInfo>();
            ResponseMessages = new List<string>();
        }

        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public InvDepartmentRequisitionInfo InvDepartmentRequisition { get; set; }
        public List<InvDepartmentRequisitionInfo> InvDepartmentRequisitions { get; set; }

        // Added for DataTables support
        public int TotalRecordCount { get; set; }
        public int FilteredRecordCount { get; set; }
    }
}
