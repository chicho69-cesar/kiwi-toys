using System.ComponentModel.DataAnnotations;

namespace KiwiToys.Data.Entities {
    public class Country {
        public int Id { get; set; }

        [Required(ErrorMessage = "El {0} es obligatorio")]
        [MaxLength(100, ErrorMessage = "El {0} debe tener maximo {1} caracteres")]
        [Display(Name = "País")]
        public string Name { get; set; }

        public ICollection<State> States { get; set; }

        [Display(Name = "Estados")]
        public int StatesNumber =>
            States == null ? 0 : States.Count;

        [Display(Name = "Ciudades")]
        public int CitiesNumber =>
            States == null ? 0 : States.Sum(s => s.CitiesNumber);
    }
}