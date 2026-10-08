using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvSupplier
{
    public class InvSupplierInfo
    {
        public string RowId { get; set; }

        public int SupplierId { get; set; }

        public string SupplierName { get; set; }

        public string ContactPerson { get; set; }

        public string ContactDesignation { get; set; }

        public string MailingAddress { get; set; }

        public string BillingAddress { get; set; }

        public string OfficePhone { get; set; }

        public string Mobile { get; set; }

        public string Email { get; set; }

        public string Web { get; set; }

        public string ItemId { get; set; }

        public string DepartmentId { get; set; }

        public int IsForeignSupplier { get; set; }

        public decimal? ComissionPercent { get; set; }

        public string CountryId { get; set; }
        public virtual string CountryName { get; set; }
        public virtual string DepartmentName { get; set; }

    }
}