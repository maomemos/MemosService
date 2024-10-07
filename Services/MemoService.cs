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

        public async Task<List<Memo>> GetMemoByPage(Query query, int page, int pageSize)
        {
            var memoQuery = _context.Memos.AsQueryable();
            // 根据 Query 对象的属性动态添加过滤条件
            if (query.memoId != -1)
            {
                memoQuery = memoQuery.Where(x => x.memoId == query.memoId);
            }
            if (query.userId != -1)
            {
                memoQuery = memoQuery.Where(x => x.userId == query.userId);
            }
            if (!string.IsNullOrEmpty(query.username))
            {
                var user = await _context.Users.Where(x => x.username == query.username).FirstOrDefaultAsync();
                if (user != null)
                {
                    memoQuery = memoQuery.Where(x => x.userId == user.userId);
                }
            }
            if (!string.IsNullOrEmpty(query.tag))
            {
                memoQuery = memoQuery.Where(x => x.tags!.Contains(query.tag));
            }
            if (!string.IsNullOrEmpty(query.content))
            {
                memoQuery = memoQuery.Where(x => x.content.Contains(query.content));
            }
            var memoList = await memoQuery
                .OrderByDescending(x => x.createdDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
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

        public async Task<Memo> PostMemoByOpenId(QQMemo qqMemo)
        {

            return null;
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
