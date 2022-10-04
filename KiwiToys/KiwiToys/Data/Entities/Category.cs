using System.ComponentModel.DataAnnotations;

namespace KiwiToys.Data.Entities {
    public class Category {
        public int Id { get; set; }

        [Required(ErrorMessage = "La {0} es obligatoria")]
        [MaxLength(50, ErrorMessage = "La {0} debe tener maximo {1} caracteres")]
        [Display(Name = "Categoria")]
        public string Name { get; set; }

        public ICollection<ProductCategory> ProductCategories { get; set; }

        [Display(Name = "# Productos")]
        public int ProductsNumber =>
            ProductCategories == null ? 0 : ProductCategories.Count;
    }
}