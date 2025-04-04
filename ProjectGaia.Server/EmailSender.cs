using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

namespace ProjectGaia.Server
{
    /// <summary>
    /// Responsável por enviar emails de forma assíncrona usando o protocolo SMTP.
    /// Utiliza as variáveis de ambiente "email" e "password" para autenticação no servidor SMTP.
    /// 
    /// Implementa a interface <see cref="IEmailSender"/> e fornece uma implementação de envio de emails.
    /// Usa a biblioteca MimeKit para compor a mensagem de email e o protocolo SMTP para envio.
    /// </summary>
    public class EmailSender : IEmailSender
    {
        /// <summary>
        /// Envia uma mensagem de email de forma assíncrona usando o protocolo SMTP.
        /// Faz uso das variáveis de ambiente "email" e "password".
        /// </summary>
        /// <param name="recipient">Endereço email do recipiente.</param>
        /// <param name="subject">Assunto da mensagem.</param>
        /// <param name="body">Corpo da mensagem.</param>
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
