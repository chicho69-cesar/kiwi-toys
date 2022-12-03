using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KiwiToys.Data.Entities {
    public class Product {
        public int Id { get; set; }

        [MaxLength(50, ErrorMessage = "El {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El {0} es obligatorio.")]
        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [DataType(DataType.MultilineText)]
        [MaxLength(500, ErrorMessage = "La {0} debe tener máximo {1} caractéres.")]
        [Display(Name = "Descripción")]
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        [Required(ErrorMessage = "El {0} es obligatorio.")]
        [Display(Name = "Precio")]
        public decimal Price { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        [Required(ErrorMessage = "El {0} es obligatorio.")]
        [Display(Name = "Stock")]
        public float Stock { get; set; }

        public ICollection<ProductCategory> ProductCategories { get; set; }

        [Display(Name = "Categorías")]
        public int CategoriesNumber =>
            ProductCategories == null ? 0 : ProductCategories.Count;

        public ICollection<ProductImage> ProductImages { get; set; }

        [Display(Name = "Fotos")]
        public int ImagesNumber =>
            ProductImages == null ? 0 : ProductImages.Count;

        // TODO: Change the path for local development
        [Display(Name = "Foto")]
        public string ImageFullPath => ProductImages == null || ProductImages.Count == 0
            ? $"https://localhost:7215/images/noimage.png"
            : ProductImages.FirstOrDefault().ImageFullPath;

        public ICollection<SaleDetail> SaleDetails { get; set; }

        public ICollection<Comment> Comments { get; set; }
    }
}