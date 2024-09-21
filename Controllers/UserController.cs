﻿using MemosService.Services;
using MemosService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;

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
            string usernamePattern = @"^[a-zA-Z0-9]{2,15}$";
            string passwordPattern = @"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[#@_])[a-zA-Z0-9#@_]{10,20}$";
            bool isValidUsername = Regex.IsMatch(auth.username, usernamePattern);
            bool isValidPassword = Regex.IsMatch(auth.password, passwordPattern);
            if (!isValidPassword || !isValidUsername)
            {
                _logger.LogError($"[UserController] 登录用户: 登录失败");
                return Json(new { account = Json(null).Value, statusCode = 400 });
            }
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
    }
}
