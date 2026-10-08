using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrDesignation
{
    public class HrDesignationInfo
    {
        public string RowId { get; set; }
        public Int32 DesignationId { get; set; }
        public string DesignationName { get; set; }
        public string ShortDesignationName { get; set; }
        public string JobDescription { get; set; }
        public string JobSpecification { get; set; }
        public Int32 Rank { get; set; }
        public Int32 NextRank { get; set; }
        public string DesignationGroupId { get; set; }
        public virtual string DesignationGroupName { get; set; }
    }
}