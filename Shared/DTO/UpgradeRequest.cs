namespace Shared.DTO
{

    public class UpgradeRequest
    {
        public int? NewSeats { get; set; }
        public int? NewPlanId { get; set; }
        public bool ApplyNow { get; set; }
    }

    public class CancelRequest 
    { 
        public bool AtPeriodEnd { get; set; } = true; 
    }

}
