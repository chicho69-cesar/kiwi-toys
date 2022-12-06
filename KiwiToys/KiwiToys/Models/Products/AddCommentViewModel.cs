using System.ComponentModel.DataAnnotations;

namespace KiwiToys.Models {
    public class AddCommentViewModel {
        public int ProductId { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Opinion")]
        public string Remark { get; set; }
    }
}