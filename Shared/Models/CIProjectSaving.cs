using Dapper.Contrib.Extensions;

namespace Shared.Models
{
    [Table("CIProjectSaving")]
    public class CIProjectSaving
    {
        [ExplicitKey]
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public string SavingsCategory { get; set; }
        public int Month { get; set; }
        public string SavingType { get; set; }
        public decimal SavingAmount { get; set; }
        public string SavingUnit { get; set; }
        public bool IsCurrency { get; set; }
        public DateTime Date { get; set; }
        public DateTime DateCreated { get; set; }
        public long CreatedBy { get; set; }
    }
}
