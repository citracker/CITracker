namespace Shared.ViewModels
{
    public class OEDashboardVM
    {
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string Currency { get; set; } = "$";

        public PipelineByStatusVM Pipeline { get; set; } = new();
        public SavingsForecastVM Forecast { get; set; } = new();
        public CumulativeSavingsVM Cumulative { get; set; } = new();
        public List<TopProjectVM> TopProjects { get; set; } = new();
        public List<ProjectHealthVM> Health { get; set; } = new();
        public DeptFacilitySavingsVM DeptFacility { get; set; } = new();
        public List<CycleTimeVM> CycleTime { get; set; } = new();
        public List<WorkloadVM> Workload { get; set; } = new();
        public CarryOverClassificationVM CarryOver { get; set; } = new();

        // KPI tiles
        public int TotalProjects { get; set; }
        public int ActiveProjects { get; set; }
        public int CompletedProjects { get; set; }
        public decimal TotalTargetSavings { get; set; }
        public decimal TotalRealizedSavings { get; set; }
        public decimal AchievementPercent => TotalTargetSavings > 0
            ? Math.Round(TotalRealizedSavings / TotalTargetSavings * 100, 1) : 0;
    }

    public class PipelineByStatusVM
    {
        // Rows = statuses, Columns = priorities
        public List<string> Statuses { get; set; } = new();
        public List<string> Priorities { get; set; } = new();
        // Key: "{status}|{priority}" → count
        public Dictionary<string, int> Cells { get; set; } = new();
    }

    public class SavingsForecastVM
    {
        public List<string> Labels { get; set; } = new();  // month labels
        public List<decimal> TargetCumulative { get; set; } = new();
        public List<decimal> ActualCumulative { get; set; } = new();
    }

    public class CumulativeSavingsVM
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> Cumulative { get; set; } = new();
        public List<decimal> Monthly { get; set; } = new();
    }

    public class TopProjectVM
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public decimal TargetSavings { get; set; }
        public string Currency { get; set; }
        public string Priority { get; set; }
        public string SponsorName { get; set; }
        public string Status { get; set; }
    }

    public class ProjectHealthVM
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public bool CarryOver { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Facilitator { get; set; }
        public string Sponsor { get; set; }
        public int DaysRemaining { get; set; }
        public string Health { get; set; }  // OnTrack | AtRisk | Overdue | Done | Cancelled
    }

    public class DeptFacilitySavingsVM
    {
        public List<string> Departments { get; set; } = new();
        public List<string> Facilities { get; set; } = new();
        public Dictionary<string, decimal> Cells { get; set; } = new();  // "{dept}|{facility}" → savings
    }

    public class CycleTimeVM
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int PlannedDays { get; set; }
        public int ElapsedDays { get; set; }
        public string Status { get; set; }
    }

    public class WorkloadVM
    {
        public long UserId { get; set; }
        public string Name { get; set; }
        public int ActiveAsFacilitator { get; set; }
        public int ActiveAsSponsor { get; set; }
        public int TotalActive => ActiveAsFacilitator + ActiveAsSponsor;
    }

    public class CarryOverClassificationVM
    {
        public List<string> Classifications { get; set; } = new();
        public Dictionary<string, int> NonCarryOver { get; set; } = new();  // classification → count
        public Dictionary<string, int> CarryOver { get; set; } = new();
    }



    // ── public return shapes ────────────────────────────────────────
    public class TopProjectRow
    {
        public string Title { get; set; }
        public decimal TargetSavings { get; set; }
        public string Currency { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
    }
    public class WorkloadRow 
    { 
        public string Name { get; set; } 
        public int Facilitator { get; set; } 
        public int Sponsor { get; set; } 
    }
    public class ForecastResult
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> TargetCumulative { get; set; } = new();
        public List<decimal> ActualCumulative { get; set; } = new();
    }
    public class CumulativeResult
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> Monthly { get; set; } = new();
        public List<decimal> Cumulative { get; set; } = new();
    }
    public class StackedResult
    {
        public List<string> Labels { get; set; } = new();
        public List<StackedDataset> Datasets { get; set; } = new();
    }
    public class StackedDataset { public string Label { get; set; } public List<decimal> Data { get; set; } = new(); }
    public class CycleTimeResult
    {
        public List<string> Labels { get; set; } = new();
        public List<int> Planned { get; set; } = new();
        public List<int> Elapsed { get; set; } = new();
    }
    public class HealthRow
    {
        public string Title { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public string Facilitator { get; set; }
        public string Sponsor { get; set; }
        public DateTime EndDate { get; set; }
        public bool CarryOver { get; set; }
        public int DaysRemaining { get; set; }
        public string Health { get; set; }
    }

    // ── row DTOs ────────────────────────────────────────────────
    public class StatusRow { 
        public string Status { get; set; } 
        public int Cnt { get; set; } 
    }
    public class CarryRow
    {
        public string Classification { get; set; }
        public int Total { get; set; }
        public int CarryOver { get; set; }
        public int NonCarryOver { get; set; }
    }
    public class MonthRow {
        public int Yr { get; set; }
        public int Mo { get; set; }
        public string MonthYear { get; set; } 
        public decimal Savings { get; set; } 
    }
    public class DeptFacilityRow { 
        public string Department { get; set; } 
        public string Facility { get; set; } 
        public decimal Savings { get; set; } 
    }
    public class CycleTimeRow
    {
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public int PlannedDays { get; set; }
    }
}
