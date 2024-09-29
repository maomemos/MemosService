using System.ComponentModel;

namespace MemosService.Models
{
    public class User
    {
        public int userId { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string? email { get; set; }
        public string? open_id { get; set; }
        public DateTime createdDate { get; set; } = DateTime.UtcNow;
        public DateTime lastModifiedDate { get; set; } = DateTime.UtcNow;
    }
}
