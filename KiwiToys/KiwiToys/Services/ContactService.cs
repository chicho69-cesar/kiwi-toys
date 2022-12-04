using KiwiToys.Models;
using System.Net.Mail;
using System.Net;

namespace KiwiToys.Services {
    public class ContactService : IContactService {
        private readonly IConfiguration _configuration;

        public ContactService(IConfiguration configuration) {
            _configuration = configuration;
        }

        public string SendMessage(ContactViewModel contactViewModel) {
            string from = _configuration["Mail:From"];
            string name = _configuration["Mail:Name"];
            string service = _configuration["Mail:Smtp"];
            int port = int.Parse(_configuration["Mail:Port"]);
            string password = _configuration["Mail:Password"];

            try {
                MailMessage mail = new() {
                    From = new MailAddress(contactViewModel.Email)
                };

                mail.To.Add(from);
                mail.Subject = "Contacto a traves de " + name;
                mail.Body = contactViewModel.Message +
                    $"\n\nAtt: { contactViewModel.Name }";

                SmtpClient smtp = new(service) {
                    Port = port,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(from, password),
                    EnableSsl = true
                };

                smtp.Send(mail);

                return "Success";
            } catch (Exception) {
                return "Failed";
            }
        }
    }
}