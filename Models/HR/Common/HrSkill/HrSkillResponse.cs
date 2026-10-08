using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Common.HrSkill
{
    public class HrSkillResponse : WebResponse
    {
        public HrSkillResponse()
        {
            ResponseTS = DateTime.Now;
            HrSkills = new List<HrSkillInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrSkillInfo HrSkill;
        public List<HrSkillInfo> HrSkills;
    }
}