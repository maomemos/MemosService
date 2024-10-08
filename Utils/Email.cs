using System.Net;
using System.Net.Mail;
using MemosService.Data;

namespace MemosService.Utils
{
    public class Email : IEmail
    {
        private readonly IConfiguration _config;
        private readonly MemosContext _context;
        public Email()
        {

        }
        public Email(IConfiguration configuration, MemosContext memosContext) 
        { 
            _config = configuration;
            _context = memosContext;
        }
        private async Task SendMailMessage(string email, string subject, string messageBody)
        {
            MailMessage message = new MailMessage();
            message.From = new MailAddress(_config["Email:Username"]!, "donotreply");
            message.To.Add(email);
            message.Subject = subject + $": {email}";
            message.Body = messageBody;
            message.IsBodyHtml = false;
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = _config["Email:Host"]!;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.Port = int.Parse(_config["Email:Port"]!);
            smtpClient.EnableSsl = true;
            // 不和请求一块发送
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Timeout = 10000;
            smtpClient.Credentials = new NetworkCredential(_config["Email:Username"]!, _config["Email:Password"]);
            smtpClient.Send(message);
        }
        public async Task<bool> GetUsername(string email)
        {
            var user = _context.Users.Where(x => x.email == email).FirstOrDefault();
            var message = "";
            if (user != null) 
            {
                message = $"Hi,{email}\n您的用户名是：{user.username}";
                try
                {
                    await SendMailMessage(email, "【MAOJI】找回用户名", message);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public async Task<bool> GetResetPasswordLink(string email)
        {
            var user = _context.Users.Where(x => x.email == email).FirstOrDefault();
            var message = "";
            if (user != null)
            {
                message = $"Hi,{email}\n点击下面链接找回密码\n{_config["Cors:domain"]}/forget?hash={user.password}&userId={user.userId}&email={user.email}";
                try
                {
                    await SendMailMessage(email, "【MAOJI】找回密码", message);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
    }
}
