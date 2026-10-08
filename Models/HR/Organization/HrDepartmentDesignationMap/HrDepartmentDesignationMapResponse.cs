using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrDepartmentDesignationMap
{
    public class HrDepartmentDesignationMapResponse : WebResponse
    {
        public HrDepartmentDesignationMapResponse()
        {
            ResponseTS = DateTime.Now;
            HrDepartmentDesignationMaps = new List<HrDepartmentDesignationMapInfo>();
            ResponseMessages = new List<string>();

        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrDepartmentDesignationMapInfo HrDepartmentDesignationMap;
        public List<HrDepartmentDesignationMapInfo> HrDepartmentDesignationMaps;
    }
}