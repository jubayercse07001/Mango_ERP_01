using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Common.HrCurrency
{
    public class HrCurrencyResponse : WebResponse
    {
        public  HrCurrencyResponse()
        {
            ResponseTS = DateTime.Now;
             HrCurrencys = new List< HrCurrencyInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public  HrCurrencyInfo  HrCurrency;
        public List< HrCurrencyInfo>  HrCurrencys;
    }
}