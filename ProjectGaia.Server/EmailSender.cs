using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

namespace ProjectGaia.Server
{
    public class EmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string recipient, string subject, string body)
        {
            string email = Environment.GetEnvironmentVariable("email") ?? "";
            string password = Environment.GetEnvironmentVariable("password") ?? "";

            try
            {
                MimeMessage message = new MimeMessage();
                message.From.Add(new MailboxAddress("Project Gaia", email));
                message.To.Add(new MailboxAddress(null, recipient));
                message.Subject = subject;
                message.Body = new TextPart("plain") { Text = body };

                using (SmtpClient client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 465, true);
                    await client.AuthenticateAsync(email, password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                Console.WriteLine($"Email sent successfully to: {recipient}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }
    }
}
