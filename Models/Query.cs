namespace MemosService.Models
{
    public class Query
    {
        public string content { get; set; }
        public string username { get; set; }
        public int userId { get; set; }
        public int memoId { get; set; }
        public string tag { get; set; }
        public int pageSize { get; set; } = 20;
        public int page { get; set; } = 1;
    }
}
