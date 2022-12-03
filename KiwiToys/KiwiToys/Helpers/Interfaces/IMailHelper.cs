using KiwiToys.Common;

namespace KiwiToys.Helpers {
    public interface IMailHelper {
        Response SendMail(string toName, string toEmail, string subject, string body);
    }
}