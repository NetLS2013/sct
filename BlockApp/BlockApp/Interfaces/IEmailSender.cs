using System.Threading.Tasks;

namespace BlockApp.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}