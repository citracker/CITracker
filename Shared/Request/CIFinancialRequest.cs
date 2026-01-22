
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
}
