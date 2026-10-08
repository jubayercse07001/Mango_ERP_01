using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvRefferenceInformation
{
    public class InvRefferenceInformationResponse : WebResponse
    {
        public InvRefferenceInformationResponse()
        {
            ResponseTS = DateTime.Now;
            InvRefferenceInformations = new List<InvRefferenceInformationInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public InvRefferenceInformationInfo InvRefferenceInformation;
        public List<InvRefferenceInformationInfo> InvRefferenceInformations;
    }
}