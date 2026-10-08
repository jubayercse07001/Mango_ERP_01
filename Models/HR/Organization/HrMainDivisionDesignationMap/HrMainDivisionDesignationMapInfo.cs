using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrMainDivisionDesignationMap
{
    public class HrMainDivisionDesignationMapInfo
    {
        public string RowId { get; set; }

        public Int32 MapId { get; set; }

        public string MainDivisionId { get; set; }

        public string DesignationId { get; set; }

        public List<string> DesignationIds { get; set; }

        // Virtual Display Fields
        public virtual string MainDivisionName { get; set; }

        public virtual string DesignationName { get; set; }
    }
}