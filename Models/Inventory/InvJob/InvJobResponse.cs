using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvJob
{
    public class InvJobResponse : WebResponse
    {
        public InvJobResponse()
        {
            ResponseTS = DateTime.Now;
            InvJobs = new List<InvJobInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public InvJobInfo InvJob;
        public List<InvJobInfo> InvJobs;
    }
}