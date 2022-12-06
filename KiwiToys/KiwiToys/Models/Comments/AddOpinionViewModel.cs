using System.ComponentModel.DataAnnotations;

namespace KiwiToys.Models.Comments {
    public class AddOpinionViewModel {
        [DataType(DataType.MultilineText)]
        [Display(Name = "Opinion")]
        public string Remark { get; set; }
    }
}