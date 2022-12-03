using System.ComponentModel.DataAnnotations;

namespace KiwiToys.Models {
    public class CityViewModel {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener maximo {1} caracteres")]
        [Display(Name = "Ciudad")]
        public string Name { get; set; }

        public int StateId { get; set; }
    }
}