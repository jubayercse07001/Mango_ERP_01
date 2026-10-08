using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Employee.EmployeePicture
{
    public class EmployeePictureResponse : WebResponse
    {
        public EmployeePictureResponse()
        {
            ResponseTS = DateTime.Now;
            EmployeePictures = new List<EmployeePictureInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public EmployeePictureInfo EmployeePicture;
        public List<EmployeePictureInfo> EmployeePictures;
    }
}