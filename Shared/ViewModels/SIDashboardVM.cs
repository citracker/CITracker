namespace Shared.ViewModels
{
    public class SIDashboardVM
    {
    }

    public class StatusCountRow { 
        public string Status { get; set; } 
        public int Cnt { get; set; } 
    }

    public class SIOwnerDeptRow { 
        public string Department { get; set; } 
        public string Owner { get; set; } 
        public decimal Roi { get; set; } 
    }

    public class SICycleRow
    {
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public string OwnerName { get; set; }
        public int PlannedDays { get; set; }
    }

    public class SITopRow
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public string OwnerName { get; set; }
        public string Department { get; set; }
        public decimal Roi { get; set; }
    }
    
    public class SIWorkloadRow { public string Name { get; set; } public int AsOwner { get; set; } public int AsSponsor{ get;set; } }
    
    public class SIForecastResult
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> TargetCumulative { get; set; } = new();
        public List<decimal> ActualCumulative { get; set; } = new();
    }
    
    public class SIWaterfallResult
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> Deltas { get; set; } = new();
        public List<decimal> Cumulative { get; set; } = new();
    }
    
    public class SIStackedResult
    {
        public List<string> Labels { get; set; } = new();
        public List<SIStackedDataset> Datasets { get; set; } = new();
    }
    public class SIStackedDataset { public string Label { get; set; } public List<decimal> Data { get; set; } = new(); }
    
    public class SICycleTimeResult
    {
        public List<string> Labels { get; set; } = new();
        public List<int> Planned { get; set; } = new();
        public List<int> Elapsed { get; set; } = new();
        public List<string> Status { get; set; } = new();
    }
    
    public class SIHealthRow
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Owner { get; set; }
        public string Sponsor { get; set; }
        public string Department { get; set; }
        public decimal Roi { get; set; }
        public int SubProjectCount { get; set; }
        public int DaysRemaining { get; set; }
        public string Health { get; set; }
    }
    
    public class SIStatusBreakdownRow
    {
        public string Status { get; set; }
        public int InitiativeCount { get; set; }
        public decimal Roi { get; set; }
    }
}
