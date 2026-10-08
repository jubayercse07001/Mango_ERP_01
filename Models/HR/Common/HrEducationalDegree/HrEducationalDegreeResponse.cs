using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Common.HrEducationalDegree
{
    public class HrEducationalDegreeResponse : WebResponse
    {
        public HrEducationalDegreeResponse()
        {
            ResponseTS = DateTime.Now;
            HrEducationalDegrees = new List<HrEducationalDegreeInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrEducationalDegreeInfo HrEducationalDegree;
        public List<HrEducationalDegreeInfo> HrEducationalDegrees;
    }
}