using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrDesignation
{
    public class HrDesignationResponse : WebResponse
    {
        public HrDesignationResponse()
        {
            ResponseTS = DateTime.Now;
            HrDesignations = new List<HrDesignationInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrDesignationInfo HrDesignation;
        public List<HrDesignationInfo> HrDesignations;
    }
}