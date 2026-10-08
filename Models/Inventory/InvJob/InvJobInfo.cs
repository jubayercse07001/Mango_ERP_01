using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.Models.Inventory.InvJob
{
    public class InvJobInfo
    {
        public string RowId { get; set; }
        public int JobId { get; set; }
        public string JobNo { get; set; }
        public string JobName { get; set; }
        public string JobDescription { get; set; }
        public string DepartmentId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public virtual string DepartmentName { get; set; }
    }
}