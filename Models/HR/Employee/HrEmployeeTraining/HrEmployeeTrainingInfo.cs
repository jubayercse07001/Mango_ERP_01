using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Employee.HrEmployeeTraining
{
    public class HrEmployeeTrainingInfo
    {
        public string RowId { get; set; }
        public int TrainingId { get; set; }
        public string EmployeeId { get; set; }
        public string TrainingSource { get; set; }
        public string TrainingListId { get; set; }
        public string Trainer { get; set; }
        public string InstituteName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Duration { get; set; }
        public virtual string EmployeeName { get; set; }
        public virtual string TrainingTitle { get; set; }
    }
}