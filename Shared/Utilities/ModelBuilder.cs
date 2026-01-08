using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Utilities
{
    public class ModelBuilder
    {
        public static AuditLog BuildAuditLog(string action, string description, string email)
        {
            return new AuditLog
            {
                Action = action,
                Description = description,
                DateCreated = DateTime.UtcNow,
                CreatedBy = email
            };
        }
    }
}
