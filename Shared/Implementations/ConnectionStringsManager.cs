using Microsoft.Extensions.Options;
using Shared.Interfaces;

namespace Shared.Implementations
{
    public class ConnectionStringsManager : IConnectionStringsManager
    {
        private readonly KeyValues keyValues;

        public ConnectionStringsManager(IOptions<KeyValues> options)
        {
            keyValues = options.Value;
        }

        public async Task<string> SQLDBConnection() => await Task.FromResult(keyValues.SQLDBConnection);
    }
}
