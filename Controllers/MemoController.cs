using Microsoft.AspNetCore.Mvc;
using MemosService.Services;
using Microsoft.AspNetCore.Authorization;
using MemosService.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

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
        /// <param name="page">页数</param>
        /// <param name="pageSize">每页条目数</param>
        /// <returns></returns>
        [HttpGet("/Memo/trends", Name = "GetMemoByPage")]
        [Authorize]
        public async Task<IActionResult> GetMemoByPage([FromQuery] int page, int pageSize) 
        {
            var memoList = await _memoService.GetMemoByPage(page, pageSize);
            if(memoList == null)
            {
                _logger.LogError($"[MemoController] 查询 Memo Page: 每页展示 {pageSize} 条 memo, 第 {page} 页为空");
                return Json(new { memoList = memoList, statusCode = 400 });
            }
            return Json(new { memoList = memoList, statusCode = 200 });
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
            var idUsername = await _userService.GetUserById(memo.userId);
            if (idUsername == null)
            {
                _logger.LogError($"[MemoController] Post Memo: 发送失败");
                return Json(new { memo = memo, message = "权限错误，发送失败", statusCode = 400 });
            }
            else
            {
                if(idUsername.username != username)
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
    }
}
