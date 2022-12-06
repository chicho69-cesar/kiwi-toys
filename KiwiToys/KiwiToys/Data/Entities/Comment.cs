using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KiwiToys.Data.Entities {
    public class Comment {
        public int Id { get; set; }

        public User User { get; set; }

        public Product Product { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd hh:mm tt}")]
        [Required(ErrorMessage = "La {0} es obligatorio.")]
        [Display(Name = "Fecha")]
        public DateTime Date { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Opinion")]
        public string Remark { get; set; }
    }
}