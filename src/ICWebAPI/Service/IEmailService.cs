using System.Threading.Tasks;

namespace ICWebAPI.Service
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}