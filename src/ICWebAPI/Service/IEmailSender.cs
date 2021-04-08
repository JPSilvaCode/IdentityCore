using System.Threading.Tasks;

namespace ICWebAPI.Service
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}