using KiwiToys.Common;
using KiwiToys.Data.Entities;

namespace KiwiToys.Models {
    public class NewsViewModel {
        public PaginatedList<News> News { get; set; }
    }
}