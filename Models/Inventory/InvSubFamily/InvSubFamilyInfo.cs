using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvSubFamily
{
    public class InvSubFamilyInfo
    {
        public string RowId { get; set; }
        public int SubFamilyId { get; set; }
        public string SubFamilyName { get; set; }
        public string FamilyId { get; set; }
        public virtual string FamilyName { get; set; }
    }
}