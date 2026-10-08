using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrDepartmentDesignationMap
{
    public class HrDepartmentDesignationMapInfo
    {
        public string RowId { get; set; }

        public Int32 MapId { get; set; }

        public string DepartmentId { get; set; }

        public string DesignationId { get; set; }

        public List<string> DesignationIds { get; set; }

        // Virtual Display Fields
        public virtual string DepartmentName { get; set; }

        public virtual string DesignationName { get; set; }
    }
}