using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Common.HrSkill
{
    public class HrSkillInfo
    {
        public string RowId { get; set; }

        public string SkillTypeId { get; set; }
        public Int32 SkillId { get; set; }
        public string SkillName { get; set; }
        public string Description { get; set; }
        

        public virtual string SkillTypeName { get; set; }
    }
}