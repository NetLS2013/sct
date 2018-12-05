using System;
using System.Net;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BlockApp.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BlockApp.Services
{
    public class EmailSender : IEmailSender
    {
        public IConfiguration Configuration { get; set; }

        public EmailSender(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var client = new SmtpClient(Configuration["EmailSmtp:Host"], Convert.ToInt32(Configuration["EmailSmtp:Port"]))
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(Configuration["EmailSmtp:UserName"], Configuration["EmailSmtp:Password"]),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };
            
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(Configuration["EmailSmtp:Email"]),
                    To = { email },
                    Subject = subject,
                    Body = message
                };
            
                await client.SendMailAsync(mailMessage);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
