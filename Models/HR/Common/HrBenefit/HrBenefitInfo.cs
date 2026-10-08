using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Common.HrBenefit
{
    public class HrBenefitInfo
    {
        public string BenefitId { get; set; }
        public string BenefitName { get; set; }
        public string Description { get; set; }

        public bool IsPercent { get; set; }
        public decimal PercentOfBasicSalary { get; set; }
        public decimal PercentOfGrossSalary { get; set; }
        public decimal FixedAmount { get; set; }

        public string CurrencyId { get; set; }

        public int PaymentCalculationRatio { get; set; }
        public int Rank { get; set; }
        public int ImplementationProcess { get; set; }
        public int DomainStatus { get; set; }

        // Audit Fields
        public string CreateUser { get; set; }
        public string UpdateUser { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}