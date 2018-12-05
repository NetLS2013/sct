using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BlockApp.Interfaces;

namespace BlockApp.Extensions
{
    public static class EmailSenderExtensions
    {
        public static Task SendEmailConfirmationAsync(this IEmailSender emailSender, string email, string link)
        {
            return emailSender.SendEmailAsync(email, "Confirm your email",
                $"Please confirm your account by clicking this link: <a href='{link}'>link</a>");
        }
        
        public static Task SendEmailTransactionAsync(this IEmailSender emailSender, string email, string message)
        {
            return emailSender.SendEmailAsync(email, "Safecryptotrades Notification", message);
        }
    }
}
