using MemosService.Models;
using Microsoft.EntityFrameworkCore;
using MemosService.Data;
using MemosService.Utils;

namespace MemosService.Services
{
    public class UserService : IUserService
    {
        private readonly MemosContext _context;
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _config;
        public UserService(MemosContext context, ILogger<UserService> logger, IConfiguration configuration) 
        {
            _context = context;
            _logger = logger;
            _config = configuration;
        }

        public async Task<User> GetUserById(int userId)
        {
            var user = await _context.Users.Where(x => x.userId == userId).FirstOrDefaultAsync();
            return user;
        }

        public async Task<User> GetUserByUsername(string username)
        {
            var user = await _context.Users.Where(x => x.username == username).FirstOrDefaultAsync();
            return user;
        }

        public async Task<User> RegisterUser(Auth auth)
        {
            try
            {
                var username = auth.username;
                var password = BCrypt.Net.BCrypt.HashPassword(auth.password, 4);
                if(await _context.Users.Where(x => x.username == username).FirstOrDefaultAsync() != null)
                {
                    _logger.LogError($"[UserService] 注册用户: 用户名已存在");
                    return null;
                }
                var now = DateTime.UtcNow;
                var user = new User
                {
                    username = username,
                    password = password,
                    createdDate = now,
                    lastModifiedDate = now,
                };
                await _context.Users.AddAsync(user);
                _context.SaveChanges();
                return user;
            }
            catch
            {
                _logger.LogError($"[UserService] 注册用户: 参数获取错误");
                return null;
            }
        }
        public async Task<string> LoginUser(Auth auth)
        {
            var username = auth.username;
            var password = auth.password;
            var user = await _context.Users.Where(x => x.username == username).FirstOrDefaultAsync();
            if (user == null)
            {
                _logger.LogError($"[UserService] 登录用户: 用户不存在");
                return null;
            }
            if (!BCrypt.Net.BCrypt.Verify(password, user.password))
            {
                _logger.LogError($"[UserService] 登录用户: 密码错误");
                return null;
            }
            var tokenTool = new Token(_config);
            var token = tokenTool.GenerateToken(auth);
            return token;
        }
    }
}