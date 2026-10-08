using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Employee.HrEmployeeTrainingList
{
    public class HrEmployeeTrainingListResponse : WebResponse
    {
        public HrEmployeeTrainingListResponse()
        {
            ResponseTS = DateTime.Now;
            HrEmployeeTrainingLists = new List<HrEmployeeTrainingListInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrEmployeeTrainingListInfo HrEmployeeTrainingList;
        public List<HrEmployeeTrainingListInfo> HrEmployeeTrainingLists;
    }
}