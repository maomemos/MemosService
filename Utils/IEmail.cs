namespace MemosService.Utils
{
    public interface IEmail
    {
        Task<bool> GetUsername(string email);
        Task<bool> GetResetPasswordLink(string email);
    }
}
