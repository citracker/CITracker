using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Request
{
    public class CITeamRequest
    {    
        public long ProjectId { get; set; }
        public List<TeamMembers> Team { get; set; }
    }

    public class TeamMembers
    {
        public long UserId { get; set; }
        public string Role { get; set; }
        public bool SendNotification { get; set; }
    }

    public class TeamMembersDTO
    {
        public long? Id { get; set; }
        public long UserId { get; set; }
        public string User { get; set; }
        public string Role { get; set; }
        public bool SendNotification { get; set; }
    }

    public class CIToolRequest
    {
        public long ProjectId { get; set; }
        public string Methodology { get; set; }
        public string Tool { get; set; }
    }
    public class CITeamDTO
    {
        public long ProjectId { get; set; }
        public List<TeamMembersDTO> Team { get; set; }
    }
}
