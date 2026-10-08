using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrMainDivision
{
    public class HrMainDivisionResponse : WebResponse
    {
        public HrMainDivisionResponse()
        {
            ResponseTS = DateTime.Now;
            HrMainDivisions = new List<HrMainDivisionInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrMainDivisionInfo HrMainDivision;
        public List<HrMainDivisionInfo> HrMainDivisions;
    }
}