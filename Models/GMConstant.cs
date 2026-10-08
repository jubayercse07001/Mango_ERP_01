using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models
{
    public class GMConstant
    {
        public static string Expense_Claim_STATUS_NEW = "New";
        //public static DateTime NullDate = new DateTime(1753, 1, 1);  //GMConstant.NULL_DATE)
        //public static readonly DateTime NullDate = DateTime.Parse("1900-01-01");
        //public static readonly DateTime NullDate = Convert.ToDateTime("1900-01-01");
        public static readonly DateTime NullDate = new DateTime(9999, 12, 31);
    }
}