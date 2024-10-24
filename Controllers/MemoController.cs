using Microsoft.AspNetCore.Mvc;
using MemosService.Services;
using Microsoft.AspNetCore.Authorization;
using MemosService.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Cors;

namespace MemosService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MemoController : Controller
    {
        private readonly IMemoService _memoService;
        private readonly IUserService _userService;
        private readonly ILogger<MemoController> _logger;
        public MemoController(IMemoService memoService, IUserService userService, ILogger<MemoController> logger)
        {
            _memoService = memoService;
            _userService = userService;
            _logger = logger;
        }

        // GET: /Memo?id
        /// <summary>
        /// 查询条目
        /// </summary>
        /// <param name="memoId">Memo Id</param>
        /// <returns></returns>
        [HttpGet("/Memo", Name = "GetMemoById")]
        [Authorize]
        public async Task<IActionResult> GetMemoById([FromQuery] int memoId)
        {
            var memo = await _memoService.GetMemoById(memoId);
            if (memo == null)
            {
                _logger.LogError($"[MemoController] 查询 Memo: 不存在 memoId 为 {memoId} 的笔记");
                return Json(new { memo = memo, statusCode = 400 });
            }
            return Json(new { memo = memo, statusCode = 200 });
        }

        /// <summary>
        /// 查询特定页条目
        /// </summary>
        /// <param name="query">查询参数</param>
        /// <returns></returns>
        [HttpPost("/Memo/trends", Name = "GetMemoByPage")]
        [Authorize]
        public async Task<IActionResult> GetMemoByPage([FromBody] Query query) 
        {
            var memoList = await _memoService.GetMemoByPage(query, query.page, query.pageSize);
            if(memoList == null)
            {
                _logger.LogError($"[MemoController] 查询 Memo Page: 每页展示 {query.pageSize} 条 memo, 第 {query.page} 页为空");
                return Json(new { memoList = memoList, statusCode = 400 });
            }
            return Json(new { memoList = memoList, statusCode = 200 });
        }

        /// <summary>
        /// 发送 Memo
        /// </summary>
        /// <param name="qqMemo">qq 消息里的参数</param>
        /// <returns></returns>
        [HttpPost("/Memo/bot", Name = "PostMemoByOpenId")]
        [EnableCors("*")]
        public async Task<IActionResult> PostMemoByOpenId([FromBody] QQMemo qqMemo)
        {
            var user = await _userService.GetUserByOpenId(qqMemo.open_id);
            Memo memo = new Memo();
            if (user == null)
            {
                _logger.LogError($"[MemoController] Post Memo: 发送失败");
                return Json(new { memo = memo, message = "发送失败", statusCode = 400 });
            }
            memo.content = qqMemo.memo;
            memo.userId = user.userId;
            var result = await _memoService.PostMemo(memo);
            if (result == null)
            {
                _logger.LogError($"[MemoController] Post Memo: 发送失败");
                return Json(new { memo = memo, message = "发送失败", statusCode = 400 });
            }
            return Json(new { memo = memo, statusCode = 200 });
        }

        /// <summary>
        /// 发送 Memo 或更新 Memo
        /// </summary>
        /// <param name="memo">Memo 对象</param>
        /// <returns></returns>
        [HttpPost("/Memo", Name = "PostMemo")]
        [Authorize]
        public async Task<IActionResult> PostMemo([FromBody] Memo memo)
        {
            var token = Request.Headers["Authorization"].FirstOrDefault();
            token = token?.Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);
            var claims = jsonToken.Claims.ToList();
            var username = claims.FirstOrDefault(c => c.Type == "sub")!.Value.ToString();
            var originMemo = await _memoService.GetMemoById(memo.memoId);
            
            if (originMemo == null)
            {
                var userId = memo.userId;
                var user =  await _userService.GetUserById(userId);
                if(user == null)
                {
                    _logger.LogError($"[MemoController] Post Memo: 发送失败");
                    return Json(new { memo = memo, message = "发送失败", statusCode = 400 });
                }
                var result = await _memoService.PostMemo(memo);
                if (result == null)
                {
                    _logger.LogError($"[MemoController] Post Memo: 发送失败");
                    return Json(new { memo = memo, message = "发送失败", statusCode = 400 });
                }
                return Json(new { memo = memo, statusCode = 200 });
            }
            else
            {
                var user = await _userService.GetUserById(originMemo.userId);
                if (user == null) 
                {
                    _logger.LogError($"[MemoController] Post Memo: 发送失败");
                    return Json(new { memo = memo, message = "发送失败", statusCode = 400 });
                }
                else if(user.username != username)
                {
                    _logger.LogError($"[MemoController] Post Memo: 发送失败");
                    return Json(new { memo = memo, message = "权限错误，发送失败", statusCode = 400 });
                }
                else
                {
                    var result = await _memoService.PostMemo(memo);
                    if (result == null)
                    {
                        _logger.LogError($"[MemoController] Post Memo: 发送失败");
                        return Json(new { memo = memo, message = "发送失败", statusCode = 400 });
                    }
                    return Json(new { memo = memo, statusCode = 200 });
                }
            }
        }

        /// <summary>
        /// 删除 Memo
        /// </summary>
        /// <param name="memoId">Memo Id</param>
        /// <returns></returns>
        [HttpDelete("/Memo", Name = "DeleteMemo")]
        [Authorize]
        public async Task<IActionResult> DeleteMemo([FromQuery] int memoId)
        {
            var token = Request.Headers["Authorization"].FirstOrDefault();
            token = token?.Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);
            var claims = jsonToken.Claims.ToList();
            var username = claims.FirstOrDefault(c => c.Type == "sub")!.Value.ToString();
            var originMemo = await _memoService.GetMemoById(memoId);
            if (originMemo == null)
            {
                _logger.LogError($"[MemoController] 删除 Memo: 不存在 memoId 为 {memoId} 的笔记");
                return Json(new { count = 0, statusCode = 400 });
            }
            else
            {
                var idUsername = await _userService.GetUserById(originMemo.userId);
                if (idUsername == null)
                {
                    _logger.LogError($"[MemoController] 删除 Memo: 删除失败");
                    return Json(new { count = 0, statusCode = 400 });
                }
                else if (idUsername.username != username)
                {
                    _logger.LogError($"[MemoController] 删除 Memo: 删除失败");
                    return Json(new { count = 0, statusCode = 400 });
                }
                else
                {
                    var count = await _memoService.DeleteMemo(memoId);
                    if (count == 0)
                    {
                        _logger.LogError($"[MemoController] 删除 Memo: 不存在 memoId 为 {memoId} 的笔记");
                        return Json(new { count = count, statusCode = 400 });
                    }
                    return Json(new { count = count, statusCode = 200 });
                }
            }
        }
    }
}
