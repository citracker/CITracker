namespace Shared.DTO
{
    public class SIFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Priority { get; set; }
        public long? UserId { get; set; }
        public long? CountryId { get; set; }
        public long? DepartmentId { get; set; }
        public string Status { get; set; }

        /// <summary>SQL fragment applied to StrategicInitiative alias.</summary>
        public string WhereSql(string alias = "si")
        {
            var sb = new System.Text.StringBuilder();
            if (StartDate.HasValue) sb.Append($" AND {alias}.StartDate >= @StartDate");
            if (EndDate.HasValue) sb.Append($" AND {alias}.EndDate   <= @EndDate");
            if (!string.IsNullOrEmpty(Priority))
                sb.Append($" AND {alias}.Priority = @Priority");
            if (UserId.HasValue) sb.Append($" AND ({alias}.OwnerId = @UserId OR {alias}.ExecutiveSponsorId = @UserId)");
            if (CountryId.HasValue) sb.Append($" AND {alias}.OrganizationCountryId = @CountryId");
            if (DepartmentId.HasValue)
                sb.Append($" AND {alias}.OrganizationDepartmentId = @DepartmentId");
            if (!string.IsNullOrEmpty(Status))
                sb.Append($" AND {alias}.Status = @Status");
            return sb.ToString();
        }

        public object Params(int orgId) => new
        {
            OrgId = orgId,
            StartDate,
            EndDate,
            Priority,
            UserId,
            CountryId,
            DepartmentId,
            Status
        };
    }
}
