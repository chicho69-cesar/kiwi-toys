using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KiwiToys.Models {
    public class ContactViewModel {
        [Display(Name = "Nombres")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Name { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Debes ingresar un correo válido.")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Email { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Mensaje")]
        public string Message { get; set; }
    }
}