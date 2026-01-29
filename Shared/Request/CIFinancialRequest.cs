
using Shared.Request;

namespace Shared.Request
{
    public class CIFinancialRequest
    {    
        public long ProjectId { get; set; }
        public List<HardSavings> Hard { get; set; }
        public List<SoftSavings> Soft { get; set; }
        public bool OneTimeSaving { get; set; }
        public bool CarryOverSaving { get; set; }
        public DateTime FinancialVerificationDate { get; set; }
    }

    public class HardSavings
    {
        public DateTime Date { get; set; }
        public string SavingType { get; set; }
        public decimal SavingValue { get; set; }
    }

    public class SoftSavings
    {
        public decimal SavingValue { get; set; }
        public string Category { get; set; }
        public string SavingUnit { get; set; }
    }

    public class HardSavingsDTO
    {
        public long? Id { get; set; }
        public DateTime Date { get; set; }
        public string SavingType { get; set; }
        public decimal SavingValue { get; set; }
        public bool IsCurrency { get; set; }
    }

    public class SavingsDTO
    {
        public long? Id { get; set; }
        public DateTime Date { get; set; }
        public string SavingType { get; set; }
        public string SavingClassification { get; set; }
        public decimal SavingValue { get; set; }
        public string Category { get; set; }
        public string SavingUnit { get; set; }
    }

    public class SoftSavingsDTO
    {
        public long? Id { get; set; }
        public decimal SavingValue { get; set; }
        public string Category { get; set; }
        public string SavingUnit { get; set; }
    }

    public class CIFinancialDTO
    {
        public long ProjectId { get; set; }
        public List<HardSavingsDTO> Hard { get; set; }
        public List<SoftSavingsDTO> Soft { get; set; }
        public bool OneTimeSaving { get; set; } = false;
        public bool CarryOverSaving { get; set; } = false;
        public bool IsAudited { get; set; } = false;
        public long? Auditor { get; set; }
        public DateTime? AuditedDate { get; set; }
        public DateTime? FinancialVerificationDate { get; set; }
        public string? FinancialReportComment { get; set; }
    }
}
