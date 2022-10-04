using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KiwiToys.Data.Entities {
    public class City {
        public int Id { get; set; }

        [Required(ErrorMessage = "El {0} es obligatorio")]
        [MaxLength(120, ErrorMessage = "El {0} debe tener maximo {1} caracteres")]
        [Display(Name = "Estado")]
        public string Name { get; set; }

        [JsonIgnore]
        public State State { get; set; }

        public ICollection<User> Users { get; set; }
    }
}