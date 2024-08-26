using MemosService.Models;

namespace MemosService.Utils
{
    public interface IToken
    {
        string GenerateToken(Auth auth);
    }
}