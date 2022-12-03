namespace KiwiToys.Helpers {
    public interface IBodyMailHelper {
        string GetConfirmEmailMessage(string tokenLink);
        string GetResetPasswordMessage(string link);
    }
}