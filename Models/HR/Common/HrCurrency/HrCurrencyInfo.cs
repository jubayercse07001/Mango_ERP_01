using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Common.HrCurrency
{
    public class HrCurrencyInfo
    {
        public string RowId { get; set; }

        public string CountryId { get; set; }
        public Int32 CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencySymbol { get; set; }
        public Int32 Rank { get; set; }

        public virtual string CountryName { get; set; }
    }
}