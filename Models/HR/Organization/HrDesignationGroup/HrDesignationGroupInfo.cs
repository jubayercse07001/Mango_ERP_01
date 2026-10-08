using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrDesignationGroup
{
    public class HrDesignationGroupInfo
    {
        public string RowId { get; set; }
        public int DesignationGroupId { get; set; }
        public string DesignationGroup { get; set; }
        public string Phone { get; set; }
        public int Rank { get; set; }
    }
}