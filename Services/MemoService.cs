using MemosService.Data;
using MemosService.Models;
using Microsoft.EntityFrameworkCore;

namespace MemosService.Services
{
    public class MemoService : IMemoService
    {
        private readonly MemosContext _context;
        private readonly ILogger<MemoService> _logger;
        public MemoService(MemosContext context, ILogger<MemoService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Memo> GetMemoById(int memoId)
        {
            var memo = await _context.Memos.Where(x => x.memoId == memoId).FirstOrDefaultAsync();
            return memo;
        }

        public async Task<List<Memo>> GetMemoByPage(int page, int pageSize)
        {
            // 由新到旧排序
            var memoList = await _context.Memos.OrderByDescending(x => x.createdDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return memoList;
        }

        // TODO 处理 memoID 相同时的更新 Memo
        public async Task<Memo> PostMemo(Memo memo)
        {
            try
            {
                await _context.Memos.AddAsync(memo);
                _context.SaveChanges();
                return memo;
            }
            catch
            {
                _logger.LogError($"[MemoService] 添加 Memo 失败: 参数错误");
                return null;
            }
        }
    }
}
