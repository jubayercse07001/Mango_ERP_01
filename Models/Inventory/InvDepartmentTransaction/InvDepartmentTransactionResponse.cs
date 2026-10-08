using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvDepartmentTransaction
{
    public class InvDepartmentTransactionResponse : WebResponse
    {
        public InvDepartmentTransactionResponse()
        {
            ResponseTS = DateTime.Now;
            InvDepartmentTransactions = new List<InvDepartmentTransactionInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public InvDepartmentTransactionInfo InvDepartmentTransaction;
        public List<InvDepartmentTransactionInfo> InvDepartmentTransactions;
    }
}