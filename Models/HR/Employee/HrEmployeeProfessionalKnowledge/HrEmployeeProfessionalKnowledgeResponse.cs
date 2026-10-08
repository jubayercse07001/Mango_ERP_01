using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Employee.HrEmployeeProfessionalKnowledge
{
    public class HrEmployeeProfessionalKnowledgeResponse : WebResponse
    {
        public HrEmployeeProfessionalKnowledgeResponse()
        {
            ResponseTS = DateTime.Now;
            HrEmployeeProfessionalKnowledges = new List<HrEmployeeProfessionalKnowledgeInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrEmployeeProfessionalKnowledgeInfo HrEmployeeProfessionalKnowledge;
        public List<HrEmployeeProfessionalKnowledgeInfo> HrEmployeeProfessionalKnowledges;
    }
}