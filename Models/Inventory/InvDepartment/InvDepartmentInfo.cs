using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.Models.Inventory.InvDepartment
{
    public class InvDepartmentInfo
    {
        public string RowId { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string ShortDeptName { get; set; }
        
    }
}