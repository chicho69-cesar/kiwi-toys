using System.ComponentModel.DataAnnotations;

namespace KiwiToys.Data.Entities {
    public class SaleDetail {
        public int Id { get; set; }

        public Sale Sale { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Comentarios")]
        public string Remarks { get; set; }

        public Product Product { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        [Required(ErrorMessage = "La {0} es obligatorio.")]
        [Display(Name = "Cantidad")]
        public float Quantity { get; set; }

        [DisplayFormat(DataFormatString = "{0:C2}")]
        [Display(Name = "Valor")]
        public decimal Value =>
            Product == null ? 0 : (decimal)Quantity * Product.Price;
    }
}