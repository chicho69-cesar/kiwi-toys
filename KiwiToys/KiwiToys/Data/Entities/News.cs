using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KiwiToys.Data.Entities {
    public class News {
        public int Id { get; set; }

        public User User { get; set; }

        [Display(Name = "Titulo")]
        public string Title { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd hh:mm tt}")]
        [Required(ErrorMessage = "La {0} es obligatorio.")]
        [Display(Name = "Fecha")]
        public DateTime Date { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Descripcion")]
        public string Description { get; set; }

        [Display(Name = "Imagen")]
        public Guid ImageId { get; set; }

        // TODO: Change the url for the url of production
        [Display(Name = "Imagen")]
        public string ImageFullPath => ImageId == Guid.Empty
            ? $"https://localhost:7215/images/noimage.png"
            : $"https://kiwitoys.blob.core.windows.net/news/{ImageId}";
    }
}