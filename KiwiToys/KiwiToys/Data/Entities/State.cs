using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KiwiToys.Data.Entities {
    public class State {
        public int Id { get; set; }

        [Required(ErrorMessage = "El {0} es obligatorio")]
        [MaxLength(100, ErrorMessage = "El {0} debe tener maximo {1} caracteres")]
        [Display(Name = "Estado")]
        public string Name { get; set; }

        [JsonIgnore]
        public Country Country { get; set; }

        public ICollection<City> Cities { get; set; }

        [Display(Name = "Ciudades")]
        public int CitiesNumber => Cities == null ? 0 : Cities.Count;
    }
}