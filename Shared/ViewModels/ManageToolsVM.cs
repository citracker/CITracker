using Shared.ExternalModels;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ViewModels
{
    public class ManageToolsVM
    {
        public List<SiteInfo> Sites { get; set; }
        public List<MethodologyPhase> Phases { get; set; }
    }
}
