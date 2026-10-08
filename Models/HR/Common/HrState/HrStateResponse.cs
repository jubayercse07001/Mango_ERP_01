using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Common.HrState
{
    public class HrStateResponse : WebResponse
    {
        public HrStateResponse()
        {
            ResponseTS = DateTime.Now;
            HrStates = new List<HrStateInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrStateInfo HrState;
        public List<HrStateInfo> HrStates;
    }
}