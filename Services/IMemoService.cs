using MemosService.Models;

namespace MemosService.Services
{
    public interface IMemoService
    {
        Task<Memo> GetMemoById(int memoId);
        Task<List<Memo>> GetMemoByPage(Query query, int page, int pageSize);
        Task<Memo> PostMemo(Memo memo);
        Task<int> DeleteMemo(int memoId);
    }
}
