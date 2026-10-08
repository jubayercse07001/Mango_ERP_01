using ERP.Models.HR.Employee.HrEmployeeReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Employee.HrEmployeeReferenceReference
{
    public class HrEmployeeReferenceResponse : WebResponse
    {
        public HrEmployeeReferenceResponse()
        {
            ResponseTS = DateTime.Now;
            HrEmployeeReferences = new List<HrEmployeeReferenceInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrEmployeeReferenceInfo HrEmployeeReference;
        public List<HrEmployeeReferenceInfo> HrEmployeeReferences;
    }
}