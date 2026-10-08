using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvDepartmentTransactionDetail
{
    public class InvDepartmentTransactionDetailResponse : WebResponse
    {
        public InvDepartmentTransactionDetailResponse()
        {
            ResponseTS = DateTime.Now;
            InvDepartmentTransactionDetails = new List<InvDepartmentTransactionDetailInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public InvDepartmentTransactionDetailInfo InvDepartmentTransactionDetail;
        public List<InvDepartmentTransactionDetailInfo> InvDepartmentTransactionDetails;
    }
}