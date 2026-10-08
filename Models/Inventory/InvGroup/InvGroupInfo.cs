using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvGroup
{
    public class InvGroupInfo
    {
        public string RowId { get; set; } = string.Empty;
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string GroupDescription { get; set; }
        public int Rank { get; set; }
        public int maxRank { get; set; }

    }
}