using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Employee.HrEmployeeTrainingList
{
    public class HrEmployeeTrainingListInfo
    {
        public string RowId { get; set; }
        public int TrainingListId { get; set; }
        public string TrainingCategoryId { get; set; }
        public string TrainingTitle { get; set; }
        public string Description { get; set; }
        public virtual string TrainingCategoryName { get; set; }
    }
}