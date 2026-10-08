using ERP.Models.HR.Employee.EmployeeDetail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Employee.EmployeeDetail
{
    public class HrEmployeeDetailResponse : WebResponse
    {
        public HrEmployeeDetailResponse()
        {
            ResponseTS = DateTime.Now;
            HrEmployeeDetails = new List<HrEmployeeDetailInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrEmployeeDetailInfo HrEmployeeDetail;
        public List<HrEmployeeDetailInfo> HrEmployeeDetails;
    }
}