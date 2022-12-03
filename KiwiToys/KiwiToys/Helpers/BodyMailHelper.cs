namespace KiwiToys.Helpers {
    public class BodyMailHelper : IBodyMailHelper {
        public string GetConfirmEmailMessage(string tokenLink) {
            string title = "<h1 style=\"font-size: 50px; color: #4040; margin-bottom: 20px;\">" +
                "Kiwi Toys - Confirmación de Email</h1>";

            string body = "<p style=\"font-size: 20px; line-height: 1.25; margin-bottom: 20px;\">" +
                "Para habilitar este usuario por favor " +
                "haga click en el siguiente link: </p>";

            string link = $"<a style=\"cursor: pointer; display: inline-block; padding: 10px 20px; margin: 20px 40px; text-decoration: none; " +
                $"color: #2f9ddd; font-size: 20px; border: 2px solid #2f9ddd;\"" +
                $" href=\"{tokenLink}\">Confirmar Email</a>";

            return $"{title} {body} <hr/> {link}";
        }

        public string GetResetPasswordMessage(string link) {
            string title = "<h1 style=\"font-size: 50px; color: #4040; margin-bottom: 20px;\">" +
                "Kiwi Toys - Recuperacion de contraseña</h1>";

            string body = "<p style=\"font-size: 20px; line-height: 1.25; margin-bottom: 20px;\">" +
                "Para recuperar la contraseña haga click " +
                "en el siguiente enlace: </p>";

            string button = $"<a style=\"display: block; padding: 10px 20px; margin: 20px 40px; text-decoration: none; " +
                $"color: #2f9ddd; font-size: 20px; border: 2px solid #2f9ddd;\"" +
                $" href=\"{link}\">Reset Password</a>";

            return $"{title} {body} <hr/> {button}";
        }
    }
}