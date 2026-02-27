using Microsoft.Extensions.Options;
using Shared.Interfaces;

namespace Shared.Implementations
{
    public class AppSettingsManager : IAppSettingsManager
    {
        private readonly KeyValues keyValues;

        public AppSettingsManager(IOptions<KeyValues> options)
        {
            keyValues = options.Value;
        }

        public async Task<string> SQLDBConnection() => await Task.FromResult(keyValues.SQLDBConnection);
    }
}
