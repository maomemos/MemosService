using MemosService.Models;
using Microsoft.EntityFrameworkCore;
using MemosService.Data;
using MemosService.Utils;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using System.Text.Json;

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
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
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
        public async Task<User> UpdateUser(Account account)
        {   
            var userId = account.userId;
            try
            {
                var currentUser = await _context.Users.Where(x => x.userId == account.userId).FirstOrDefaultAsync();
                if(currentUser == null)
                {
                    _logger.LogError($"[UserService] 修改用户: 用户不存在");
                    return null;
                }
                currentUser.password = account.password ?? currentUser.password;
                currentUser.email = account.email ?? currentUser.email;
                currentUser.open_id = account.open_id ?? currentUser.open_id;
                if (account.password != null)
                {
                    currentUser.password = BCrypt.Net.BCrypt.HashPassword(account.password, 4) ?? currentUser.password;
                }
                _context.SaveChanges();
                return currentUser;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.ToString());
                return null;
            }
        }

        public async Task<Dictionary<int, int>> GetUserAnalysisData(int userId, int year)
        {
            try
            {
                if(await _context.Users.Where(x => x.userId == userId).FirstOrDefaultAsync() == null)
                {
                    return null;
                }
                var memosData = await _context.Memos
                    .Where(x => x.userId == userId && x.createdDate.Year == year)
                    .GroupBy(x => x.createdDate.Month)
                    .Select(g => new { Month = g.Key, Count = g.Count() })
                    .ToListAsync();
                var result = new Dictionary<int, int>();
                for (int i = 1; i <= 12; i++)
                {
                    result[i - 1] = memosData.FirstOrDefault(g => g.Month == i)?.Count ?? 0;
                }
                return result;
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex.ToString());
                return null;
            }
        }
        public async Task<List<object>> GetUserHeatmapData(int userId, int year)
        {
            try
            {
                if (await _context.Users.Where(x => x.userId == userId).FirstOrDefaultAsync() == null)
                {
                    return null;
                }
                var heatmapData = await _context.Memos
                    .Where(x => x.userId == userId && x.createdDate.Year == year)
                    .GroupBy(x => x.createdDate.Date)
                    .Select(g => new { Date = g.Key, Count = g.Count() })
                    .ToListAsync();
                var result = new List<object>();
                int daysInYear = DateTime.IsLeapYear(year) ? 366 : 365;
                DateTime startDate = new DateTime(year, 1, 1);
                for (int i = 1; i <= daysInYear; i++)
                {
                    DateTime currentDate = startDate.AddDays(i - 1);
                    int count = heatmapData.FirstOrDefault(g => g.Date == currentDate)?.Count ?? 0;
                    result.Add(new { date = currentDate.ToString("yyyy-MM-dd"), count = count, level = Math.Floor(Math.Sqrt(count * 100) / 10) });
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return null;
            }
        }
    }
}