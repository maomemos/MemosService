using MemosService.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace MemosService.Utils
{
    public class Token : IToken
    {
        private readonly IConfiguration _config;
        public Token()
        {

        }

        public Token(IConfiguration configuration)
        {
            _config = configuration;
        }

        /// <summary>
        /// 生成 JWT
        /// </summary>
        /// <param name="auth">用户登录信息</param>
        /// <returns></returns>
        public string GenerateToken(Auth auth)
        {
            var claims = new[] {
                // TODO 改名重新发放 Token
                new Claim(JwtRegisteredClaimNames.Sub, auth.username),
            };
            var expires = DateTime.UtcNow.AddYears(1);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["JWT:Key"]!);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                Audience = _config["JWT:Audience"],
                Issuer = _config["JWT:Issuer"],
                NotBefore = DateTime.UtcNow,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        }
    }
}
