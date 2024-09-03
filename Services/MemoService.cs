using MemosService.Data;
using MemosService.Models;
using Microsoft.EntityFrameworkCore;
using System;

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

        public async Task<List<Memo>> GetMemoByPage(string query, int page, int pageSize)
        {
            var memoList = await _context.Memos.Where(x => x.content.Contains(query)).OrderByDescending(x => x.createdDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return memoList;
        }

        public async Task<Memo> PostMemo(Memo memo)
        {
            var dataMemo = await _context.Memos.Where(x => x.memoId == memo.memoId).FirstOrDefaultAsync();
            if (dataMemo != null)
            {
                try
                {
                    dataMemo.content = memo.content;
                    dataMemo.lastModifiedDate = memo.lastModifiedDate;
                    dataMemo.tags = memo.tags;
                    _context.SaveChanges();
                    return memo;
                }
                catch
                {
                    _logger.LogError($"[MemoService] 更新 Memo 失败: 参数错误");
                    return null;
                }
            }
            else
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

        public async Task<int> DeleteMemo(int memoId)
        {
            try
            {
                var count = await _context.Memos.Where(x => x.memoId == memoId).ExecuteDeleteAsync();
                return count;
            }
            catch
            {
                _logger.LogError($"[MemoService] 删除 Memo 失败: 参数错误");
                return 0;
            }
        }
    }
}
