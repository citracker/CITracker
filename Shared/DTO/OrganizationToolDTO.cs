
namespace Shared.DTO
{
    public class OrganizationToolDTO
    {
        public long Id { get; set; }
        public int OrganizationId { get; set; }
        public int ToolId { get; set; }
        public string Tool { get; set; }
        public string Phase { get; set; }
        public string PhaseId { get; set; }
        public string Url { get; set; }
        public DateTime DateCreated { get; set; }
        public long CreatedBy { get; set; }
    }
}
