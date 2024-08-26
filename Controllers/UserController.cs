using MemosService.Services;
using MemosService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MemosService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;
        public UserController(IUserService userService, ILogger<UserController> logger) 
        {
            _userService = userService;
            _logger = logger;
        }

        // GET: /User/{id}
        /// <summary>
        /// 查询用户信息
        /// </summary>
        /// <param name="userId">用户 id</param>
        /// <returns></returns>
        [HttpGet("{userId}", Name = "GetUserById")]
        [Authorize]
        public async Task<IActionResult> GetUserById(int userId)
        {
            var user = await _userService.GetUserById(userId);
            if (user == null) 
            {
                _logger.LogError($"[UserController] 查询用户: 不存在 userId 为 {userId} 的用户");
                return Json(new { account = user, statusCode = 400 });
            }
            _logger.LogInformation($"[UserController] 查询用户: userId = {userId}");
            var account = new { userId = user.userId, username = user.username, createdDate = user.createdDate, lastModifiedDate = user.lastModifiedDate };
            return Json(new { account = account, statusCode = 200 });
        }
        /// <summary>
        /// 查询用户信息
        /// </summary>
        /// <param name="username">用户名</param>
        /// <returns></returns>
        [HttpGet("/User", Name = "GetUserByUsername")]
        [Authorize]
        public async Task<IActionResult> GetUserByUsername([FromQuery] string username)
        {
            var user = await _userService.GetUserByUsername(username);
            if (user == null)
            {
                _logger.LogError($"[UserController] 查询用户: 不存在 username 为 {username} 的用户");
                return Json(new { account = user, statusCode = 400 });
            }
            _logger.LogInformation($"[UserController] 查询用户: username = {username}");
            var account = new { userId = user.userId, username = user.username, createdDate = user.createdDate, lastModifiedDate = user.lastModifiedDate };
            return Json(new { account = account, statusCode = 200 });
        }

        // POST: /User/register
        /// <summary>
        /// 注册用户
        /// </summary>
        /// <param name="auth">用户对象</param>
        /// <returns></returns>
        [HttpPost("/User/register", Name = "RegisterUser")]
        public async Task<IActionResult> RegisterUser([FromBody] Auth auth)
        {
            var user = await _userService.RegisterUser(auth);
            if(user == null)
            {
                _logger.LogError($"[UserController] 注册用户: 注册失败");
                return Json(new { account = user, statusCode = 400 });
            }
            _logger.LogInformation($"[UserController] 注册用户: username = {auth.username}");
            var account = new { userId = user.userId, username = user.username, createdDate = user.createdDate, lastModifiedDate = user.lastModifiedDate };
            return Json(new { account = account, statusCode = 200 });
        }

        /// <summary>
        /// 登录用户
        /// </summary>
        /// <param name="auth">用户对象</param>
        /// <returns></returns>
        [HttpPost("/User/login", Name = "LoginUser")]
        public async Task<IActionResult> LoginUser([FromBody] Auth auth)
        {
            var token = await _userService.LoginUser(auth);
            if(token == null)
            {
                _logger.LogError($"[UserController] 登录用户: 登录失败");
                return Json(new { token = token, message = "账号密码错误", statusCode = 400 });
            }
            _logger.LogInformation($"[UserController] 登录用户: username = {auth.username}");
            return Json(new { token = token, statusCode = 200 });
        }
    }
}
