using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class ResponseHandler
    {
        public Exception Error { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public int StatusCode { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }

        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
    }
    public class ResponseHandler<T> : ResponseHandler
    {
        public IEnumerable<T> Result { get; set; }
        public T SingleResult { get; set; }
    }
}
