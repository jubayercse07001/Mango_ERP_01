using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace ERP.Models.Inventory.InvProperty
{
    public class InvPropertyInfo
    {
        public string RowId { get; set; }
        public int PropertyId { get; set; }
        public string PropertyName { get; set; }
        public string PropertyDescription { get; set; }
        public string GroupId { get; set; }
        public virtual string GroupName { get; set; }

    }
}