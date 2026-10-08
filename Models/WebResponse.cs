using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models
{
    public class WebResponse
    {
        public int ResponseStatus { get; set; }
        public DateTime ResponseTS { get; set; }
        public string ResponseCode { get; set; }
        public int TotalRecordCount { get; set; }
        public int FilteredRecordCount { get; set; }
    }
}