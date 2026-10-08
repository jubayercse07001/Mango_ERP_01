using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrCompanyInformation
{
    public class HrCompanyInformationInfo
    {
        public string RowId { get; set; }
        public Int32 CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string ShortCompanyName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Phone3 { get; set; }
        public string Mobile1 { get; set; }
        public string Mobile2 { get; set; }
        public string Mobile3 { get; set; }
        public string Fax { get; set; }
        public string Email1 { get; set; }
        public string Email2 { get; set; }
        public string Email3 { get; set; }
        public string Web { get; set; }
        public string TIN { get; set; }
        public byte[] Logo { get; set; }
        public HttpPostedFileBase LogoFile { get; set; }
        public string LogoUrl { get; set; }
        
    }
}