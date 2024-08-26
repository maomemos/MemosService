#pragma warning disable CS8618
using System.ComponentModel;

namespace MemosService.Models
{
    public class Memo
    {
        public int memoId { get; set; }

        public string content { get; set; }
        public List<string>? tags { get; set; }
        public int userId { get; set; }
        public DateTime createdDate { get; set; } = DateTime.UtcNow;
        public DateTime lastModifiedDate { get; set; } = DateTime.UtcNow;
    }
}
