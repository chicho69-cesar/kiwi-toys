using System.ComponentModel.DataAnnotations;

namespace KiwiToys.Models {
    public class StateViewModel {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener maximo {1} caracteres")]
        [Display(Name = "Estado")]
        public string Name { get; set; }

        public int CountryId { get; set; }
    }
}