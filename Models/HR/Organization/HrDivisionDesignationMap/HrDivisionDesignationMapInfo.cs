using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrDivisionDesignationMap
{
    public class HrDivisionDesignationMapInfo
    {
        public string RowId { get; set; }

        public Int32 MapId { get; set; }

        public string DivisionId { get; set; }

        public string DesignationId { get; set; }

        public List<string> DesignationIds { get; set; }

        // Virtual Display Fields
        public virtual string DivisionName { get; set; }

        public virtual string DesignationName { get; set; }
    }
}