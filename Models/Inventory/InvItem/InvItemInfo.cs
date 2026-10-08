using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace ERP.Models
{
    public class InvItemInfo
    {
        public string RowId { get; set; }
        public int ItemId { get; set; }
        public string CategoryId { get; set; }
        public string ItemCode { get; set; }
        public string OldItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public string GroupId { get; set; }
        public string ClassId { get; set; }
        public string FamilyId { get; set; }
        public string SubFamilyId { get; set; }
        public string SerialNumber { get; set; }
        public string ModelNumber { get; set; }
        public double MinimumReOrderLevel { get; set; }
        public double MaximumReOrderLevel { get; set; }
        public string CountryId { get; set; }
        public string MinimumUnitId { get; set; }
        public string MinimumUnitName { get; set; }
        public string RegisteredUnitId { get; set; }
        public string RegisteredUnitName { get; set; }
        public string SaleUnitId { get; set; }
        public string SaleUnitName { get; set; }
        public string PurchaseUnitId { get; set; }
        public string PurchaseUnitName { get; set; }
        public string ManufacturerId { get; set; }
        public string BatchNumber { get; set; }
        public byte[] ItemImage { get; set; }
        public int IsStockItem { get; set; }
        public virtual string CategoryName { get; set; }
        public virtual string GroupName { get; set; }
        public virtual string FamilyName { get; set; }
        public virtual string ClassName { get; set; }
        public virtual string SubFamilyName { get; set; }
        public virtual string ManufacturerName { get; set; }
        public virtual string CountryName { get; set; }
    }
}