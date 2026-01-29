

namespace Shared.Request
{
    public class CICommentRequest
    {    
        public long ProjectId { get; set; }
        public List<Commentz>? Comment { get; set; }
    }

    public class Commentz
    {
        public string Comment { get; set; }
        public DateTime Date { get; set; }
    }

    public class CommentzDTO
    {
        public long Id { get; set; }
        public string Comment { get; set; }
        public DateTime Date { get; set; }
    }

    public class CICommentDTO
    {
        public long ProjectId { get; set; }
        public List<CommentzDTO> Comment { get; set; }
    }
}
