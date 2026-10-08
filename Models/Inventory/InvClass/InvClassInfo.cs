using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvClass
{
    public class InvClassInfo
    {
        public string RowId { get; set; } 
        public int ClassId { get; set; }
        public string ClassName { get; set; } 
        public string GroupId { get; set; }
        public virtual string GroupName { get; set; }
    }
}