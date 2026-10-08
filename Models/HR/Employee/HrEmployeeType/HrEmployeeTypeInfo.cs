using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrEmployeeType
{
    public class HrEmployeeTypeInfo
    {
        public string RowId { get; set; }

        public Int32 EmployeeTypeId { get; set; }

        public string EmployeeTypeName { get; set; }

        public Int32 Rank { get; set; }
    }
}