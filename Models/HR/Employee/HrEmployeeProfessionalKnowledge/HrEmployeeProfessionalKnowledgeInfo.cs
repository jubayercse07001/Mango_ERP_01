using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Employee.HrEmployeeProfessionalKnowledge
{
    public class HrEmployeeProfessionalKnowledgeInfo
    {
        public string RowId { get; set; }
        public int ProfessionalKnowledgeId { get; set; }
        public string EmployeeId { get; set; }
        public string SkillId { get; set; }
        public virtual string SkillName { get; set; }
        public virtual string Description { get; set; }
    }
}