using MemosService.Services;
using MemosService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;
using System.IdentityModel.Tokens.Jwt;

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
            var account = new { userId = user.userId, username = user.username, email = user.email, open_id = user.open_id, createdDate = user.createdDate, lastModifiedDate = user.lastModifiedDate };
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
            string usernamePattern = @"^[a-zA-Z0-9]{2,15}$";
            string passwordPattern = @"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[#@_])[a-zA-Z0-9#@_]{10,20}$";
            bool isValidUsername = Regex.IsMatch(auth.username, usernamePattern);
            bool isValidPassword = Regex.IsMatch(auth.password, passwordPattern);
            if(!isValidPassword || !isValidUsername)
            {
                _logger.LogError($"[UserController] 注册用户: 注册失败");
                return Json(new { account = Json(null).Value, statusCode = 400 });
            }
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

        // POST: /User/login
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

        [HttpGet("/User/analysis", Name = "AnalysisUser")]
        public async Task<IActionResult> GetUserAnalysisData([FromQuery] int userId, int year)
        {
            var memosData = await _userService.GetUserAnalysisData(userId, year);
            if(memosData == null)
            {
                _logger.LogError($"[UserController] 查询 MemosData：查询失败");
                return Json(new { memosData = memosData, statusCode = 400 });
            }
            return Json(new { memosData = memosData, statusCode = 200 });
        }
        [HttpGet("/User/heatmap", Name = "UserHeatmap")]
        public async Task<IActionResult> GetUserHeatmap([FromQuery] int userId, int year)
        {
            var heatmapData = await _userService.GetUserHeatmapData(userId, year);
            if (heatmapData == null)
            {
                _logger.LogError($"[UserController] 查询 HeatmapData：查询失败");
                return Json(new { memosData = heatmapData, statusCode = 400 });
            }
            return Json(new { heatmapData = heatmapData, statusCode = 200 });
        }

        // PUT: /User
        /// <summary>
        /// 修改用户密码和邮箱
        /// </summary>
        /// <param name="account">用户可修改信息对象</param>
        /// <returns></returns>
        [HttpPut("/User", Name = "UpdateUser")]
        [Authorize]
        public async Task<IActionResult> UpdateUser([FromBody] Account account)
        {
            var token = Request.Headers["Authorization"].FirstOrDefault();
            token = token?.Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);
            var claims = jsonToken.Claims.ToList();
            var username = claims.FirstOrDefault(c => c.Type == "sub")!.Value.ToString();
            var originUser = await _userService.GetUserById(account.userId);

            if(originUser == null)
            {
                _logger.LogError($"[UserController] 更新 Memo: 更新用户失败");
                return Json(new { user = originUser, statusCode = 400 });
            }
            else
            {
                if(originUser.username == username)
                {
                    if(account.currentPassword != null && BCrypt.Net.BCrypt.Verify(account.currentPassword, originUser.password))
                    {
                        if(account.password != null && account.password.Length > 0)
                        {
                            string passwordPattern = @"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[#@_])[a-zA-Z0-9#@_]{10,20}$";
                            bool isValidPassword = Regex.IsMatch(account.password, passwordPattern);
                            if (!isValidPassword)
                            {
                                _logger.LogError($"[UserController] 更新 Memo: 更新用户失败");
                                return Json(new { message = "密码格式错误", statusCode = 405 });
                            }
                        }
                        if(account.email != null)
                        {
                            var emailUser = await _userService.GetUserByEmail(account.email);
                            if(emailUser != null)
                            {
                                if(emailUser.userId != originUser.userId)
                                {
                                    return Json(new { message = "邮箱已被绑定", statusCode = 402 });
                                }
                            }
                        }
                        await _userService.UpdateUser(account);
                        return Json(new { message = "用户信息修改成功", statusCode = 200 });
                    }
                    else
                    {
                        _logger.LogError($"[UserController] 更新 Memo: 更新用户失败");
                        return Json(new { message = "当前密码错误", statusCode = 401 });
                    }
                }
                else
                {
                    _logger.LogError($"[UserController] 更新 Memo: 更新用户失败");
                    return Json(new { message = "Token 错误", statusCode = 400 });
                }
            }
        }
        
        /// <summary>
        /// 找回用户名
        /// </summary>
        /// <param name="email">账号绑定的邮箱</param>
        /// <returns></returns>
        [HttpGet("/User/username", Name = "GetUsername")]
        public async Task<IActionResult> GetUsername([FromQuery] string email)
        {
            if (await _userService.ForgetUsername(email))
            {
                return Json(new { email = email, message = "查询用户名成功", statusCode = 200 });
            }
            return Json(new { email = email, message = "检查邮箱后重试", statusCode = 400 });
        }

        /// <summary>
        /// 找回用户密码
        /// </summary>
        /// <param name="email">账号绑定的邮箱</param>
        /// <returns></returns>
        [HttpGet("/User/password", Name = "GetResetPasswordLink")]
        public async Task<IActionResult> ResetPassword([FromQuery] string email)
        {
            if (await _userService.ForgetPassword(email))
            {
               return Json(new { email = email, message = "成功发送重置密码邮件", statusCode = 200 });
            }
            return Json(new { email = email, message = "检查邮箱后重试", statusCode = 400 });
        }

        [HttpPost("/User/password", Name = "UpdatePassword")]
        public async Task<IActionResult> UpdatePassword([FromQuery] string hash, [FromQuery] string userId, [FromQuery] string email, [FromBody] string password)
        {
            var user = await _userService.GetUserById(int.Parse(userId));
            if(user == null)
            {
                return Json(new { message = "参数错误", statusCode = 400 });
            }
            if(user.password == hash && user.email == email)
            {
                if(await _userService.ResetPassword(password, user))
                {
                    return Json(new { message = "重置密码成功", statusCode = 200 });
                };
            }
            return Json(new { message = "重置密码失败", statusCode = 400 });
        }
    }
}
