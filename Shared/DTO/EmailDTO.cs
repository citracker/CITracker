using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class EmailDTO
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Phone { get; set; }
        public List<ReplyTo> Replies { get; set; }

    }

    public class ReplyTo
    {
        public string EmailAddress { get; set; }
        public string Name { get; set; }
    }
}
