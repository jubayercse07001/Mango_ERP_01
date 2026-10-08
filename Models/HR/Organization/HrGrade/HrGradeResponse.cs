using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrGrade
{
    public class HrGradeResponse : WebResponse
    {
        public HrGradeResponse()
        {
            ResponseTS = DateTime.Now;
            HrGrades = new List<HrGradeInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrGradeInfo HrGrade;
        public List<HrGradeInfo> HrGrades;
    }
}