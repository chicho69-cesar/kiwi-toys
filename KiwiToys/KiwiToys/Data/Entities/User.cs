using KiwiToys.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace KiwiToys.Data.Entities {
    public class User : IdentityUser {
        [MaxLength(20, ErrorMessage = "El {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El {0} es obligatorio.")]
        [Display(Name = "Documento")]
        public string Document { get; set; }

        [MaxLength(50, ErrorMessage = "El {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El {0} es obligatorio.")]
        [Display(Name = "Nombre(s)")]
        public string FirstName { get; set; }

        [MaxLength(50, ErrorMessage = "Los {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "Los {0} es obligatorio.")]
        [Display(Name = "Apellidos")]
        public string LastName { get; set; }

        [Display(Name = "Ciudad")]
        public City City { get; set; }

        [MaxLength(200, ErrorMessage = "La {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "La {0} es obligatorio.")]
        [Display(Name = "Dirección")]
        public string Address { get; set; }

        [Display(Name = "Foto")]
        public Guid ImageId { get; set; }

        // TODO: Change the path for local development
        [Display(Name = "Foto")]
        public string ImageFullPath => ImageId == Guid.Empty
            ? $"https://localhost:7215/images/noimage.png"
            : $"https://kiwitoys.blob.core.windows.net/users/{ImageId}";

        [Display(Name = "Tipo de usuario")]
        public UserType UserType { get; set; }

        [Display(Name = "Usuario")]
        public string FullName => 
            $"{FirstName} {LastName}";

        [Display(Name = "Usuario")]
        public string FullNameWithDocument => 
            $"{FirstName} {LastName} - {Document}";

        public ICollection<Sale> Sales { get; set; }
    }
}