using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvFamily
{
    public class InvFamilyInfo
    {
        public string RowId { get; set; }

        public int FamilyId { get; set; }

        public string FamilyName { get; set; }

        public string ClassId { get; set; }

        public virtual string ClassName { get; set; }
    }
}