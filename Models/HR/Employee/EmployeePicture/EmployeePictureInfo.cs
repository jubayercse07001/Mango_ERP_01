using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Employee.EmployeePicture
{
    public class EmployeePictureInfo
    {
        public string RowId { get; set; }
        public int HrEmployeePictureId { get; set; }
        public string EmployeeId { get; set; }
        public byte[] Picture { get; set; }
        public string PicturePath { get; set; }
        public byte[] Signature { get; set; }
        public string SignaturePath { get; set; }
        public int DomainStatus { get; set; }
    }
}