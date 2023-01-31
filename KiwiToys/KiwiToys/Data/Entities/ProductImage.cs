using System.ComponentModel.DataAnnotations;

namespace KiwiToys.Data.Entities {
    public class ProductImage {
        public int Id { get; set; }

        public Product Product { get; set; }

        [Display(Name = "Foto")]
        public Guid ImageId { get; set; }

        // TODO: Change the url for the url of production
        [Display(Name = "Foto")]
        public string ImageFullPath => ImageId == Guid.Empty
            ? $"https://localhost:7215/images/noimage.png"
            : $"https://kiwitoys.blob.core.windows.net/products/{ImageId}";
    }
}
