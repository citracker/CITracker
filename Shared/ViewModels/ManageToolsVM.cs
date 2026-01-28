using Shared.ExternalModels;
using Shared.Models;
using DriveInfo = Shared.ExternalModels.DriveInfo;

namespace Shared.ViewModels
{
    public class ManageToolsVM
    {
        public List<DriveInfo> Sites { get; set; }
        public List<MethodologyPhase> Phases { get; set; }
    }
}
