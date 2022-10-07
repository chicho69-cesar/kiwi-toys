using KiwiToys.Data.Entities;
using KiwiToys.Helpers.Interfaces;
using KiwiToys.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KiwiToys.Data {
    public class SeedDb {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly IApiService _apiService;

        public SeedDb(
            DataContext context,
            IUserHelper userHelper,
            IBlobHelper blobHelper,
            IApiService apiService
        ) {
            _context = context;
            _userHelper = userHelper;
            _blobHelper = blobHelper;
            _apiService = apiService;
        }

        public async Task SeedAsync() {
            await _context.Database.EnsureCreatedAsync();
            await CheckCategoriesAsync();
        }

        private async Task CheckCategoriesAsync() {
            if (!_context.Categories.Any()) {
                _context.Categories.Add(new Category { Name = "Electronica" });
                _context.Categories.Add(new Category { Name = "Comida" });
                _context.Categories.Add(new Category { Name = "Tecnología" });
                _context.Categories.Add(new Category { Name = "Ropa" });
                _context.Categories.Add(new Category { Name = "Gamer" });
                _context.Categories.Add(new Category { Name = "Belleza" });
                _context.Categories.Add(new Category { Name = "Nutrición" });
                _context.Categories.Add(new Category { Name = "Calzado" });
                _context.Categories.Add(new Category { Name = "Deportes" });
                _context.Categories.Add(new Category { Name = "Mascotas" });
                _context.Categories.Add(new Category { Name = "Apple" });

                await _context.SaveChangesAsync();
            }
        }
    }
}