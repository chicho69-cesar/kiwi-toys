﻿using KiwiToys.Common;
using KiwiToys.Data.Entities;

namespace KiwiToys.Models {
    public class HomeViewModel {
        public PaginatedList<Product> Products { get; set; }
        public ICollection<Category> Categories { get; set; }
        public float Quantity { get; set; }
    }
}