using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Common.HrSkillType
{
    public class HrSkillTypeResponse : WebResponse
    {
        public HrSkillTypeResponse()
        {
            ResponseTS = DateTime.Now;
            HrSkillTypes = new List<HrSkillTypeInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrSkillTypeInfo HrSkillType;
        public List<HrSkillTypeInfo> HrSkillTypes;
    }
}