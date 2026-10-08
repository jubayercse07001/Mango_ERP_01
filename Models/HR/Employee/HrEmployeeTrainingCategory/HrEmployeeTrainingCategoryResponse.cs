using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Employee.HrEmployeeTrainingCategory
{
    public class HrEmployeeTrainingCategoryResponse : WebResponse
    {
        public HrEmployeeTrainingCategoryResponse()
        {
            ResponseTS = DateTime.Now;
            HrEmployeeTrainingCategorys = new List<HrEmployeeTrainingCategoryInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrEmployeeTrainingCategoryInfo HrEmployeeTrainingCategory;
        public List<HrEmployeeTrainingCategoryInfo> HrEmployeeTrainingCategorys;
    }
}