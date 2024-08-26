using MemosService.Models;

namespace MemosService.Services
{
    public interface IMemoService
    {
        Task<Memo> GetMemoById(int memoId);
        Task<List<Memo>> GetMemoByPage(int page, int pageSize);
        Task<Memo> PostMemo(Memo memo);
    }
}
