using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvDepartmentRequisitionDetail
{
    public class InvDepartmentRequisitionDetailResponse : WebResponse
    {
        public InvDepartmentRequisitionDetailResponse()
        {
            ResponseTS = DateTime.Now;
            InvDepartmentRequisitionDetails = new List<InvDepartmentRequisitionDetailInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public InvDepartmentRequisitionDetailInfo InvDepartmentRequisitionDetail;
        public List<InvDepartmentRequisitionDetailInfo> InvDepartmentRequisitionDetails;
    }
}
