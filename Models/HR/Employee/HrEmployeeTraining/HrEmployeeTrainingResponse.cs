using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Employee.HrEmployeeTraining
{
    public class HrEmployeeTrainingResponse : WebResponse
    {
        public HrEmployeeTrainingResponse()
        {
            ResponseTS = DateTime.Now;
            HrEmployeeTrainings = new List<HrEmployeeTrainingInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrEmployeeTrainingInfo HrEmployeeTraining;
        public List<HrEmployeeTrainingInfo> HrEmployeeTrainings;
    }
}