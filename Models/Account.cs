namespace MemosService.Models
{
    public class Account
    {
        public int userId { get; set; }
        public string currentPassword { get; set; }
        public string? password { get; set; }
        public string? email { get; set; }
        public string? open_id { get; set; }
    }
}
