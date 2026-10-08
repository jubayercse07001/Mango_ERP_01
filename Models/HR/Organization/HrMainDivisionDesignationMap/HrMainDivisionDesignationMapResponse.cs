using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrMainDivisionDesignationMap
{
    public class HrMainDivisionDesignationMapResponse : WebResponse
    {
        public HrMainDivisionDesignationMapResponse()
        {
            ResponseTS = DateTime.Now;
            HrMainDivisionDesignationMaps = new List<HrMainDivisionDesignationMapInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrMainDivisionDesignationMapInfo HrMainDivisionDesignationMap;
        public List<HrMainDivisionDesignationMapInfo> HrMainDivisionDesignationMaps;
    }
}