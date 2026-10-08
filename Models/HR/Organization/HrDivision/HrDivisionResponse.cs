using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrDivision
{
    public class HrDivisionResponse : WebResponse
    {
        public HrDivisionResponse()
        {
            ResponseTS = DateTime.Now;
            HrDivisions = new List<HrDivisionInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrDivisionInfo HrDivision;
        public List<HrDivisionInfo> HrDivisions;
    }
}