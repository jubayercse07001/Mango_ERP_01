using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrSection
{
    public class HrSectionInfo
    {
        public string RowId { get; set; }
        public Int32 SectionId { get; set; }
        public string SectionName { get; set; }
        public string ShortSectionName { get; set; }
        public string DepartmentId { get; set; }
        public Int32 Rank { get; set; }

        public virtual string DepartmentName { get; set; }
    }
}