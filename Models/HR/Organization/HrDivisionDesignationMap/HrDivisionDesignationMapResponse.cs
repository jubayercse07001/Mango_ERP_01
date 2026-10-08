using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrDivisionDesignationMap
{
    public class HrDivisionDesignationMapResponse : WebResponse
    {
        public HrDivisionDesignationMapResponse()
        {
            ResponseTS = DateTime.Now;
            HrDivisionDesignationMaps = new List<HrDivisionDesignationMapInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrDivisionDesignationMapInfo HrDivisionDesignationMap;
        public List<HrDivisionDesignationMapInfo> HrDivisionDesignationMaps;
    }
}