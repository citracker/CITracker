using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ExternalModels
{
    public class SiteInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string WebUrl { get; set; }
    }

    public class DriveInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string WebUrl { get; set; }
        public string DriveType { get; set; }
        public DateTimeOffset? Created { get; set; }
        public string SiteId { get; set; }
        public string SiteUrl { get; set; }
    }
}
