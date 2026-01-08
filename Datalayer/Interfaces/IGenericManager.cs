using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datalayer.Interfaces
{
    public interface IGenericManager
    {
        Task<long> GetNextTableId(IDbConnection dbConnection, IDbTransaction transaction, string tableName);
    }
}
