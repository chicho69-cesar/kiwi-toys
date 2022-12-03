using KiwiToys.Data.Entities;
using KiwiToys.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KiwiToys.Helpers {
    public class CombosHelper : ICombosHelper {
        private readonly DataContext _context;

        public CombosHelper(DataContext context) {
            _context = context;
        }

        public async Task<IEnumerable<SelectListItem>> GetComboCategoriesAsync() {
            List<SelectListItem> list = await _context.Categories
                .Select(c => new SelectListItem {
                    Text = c.Name,
                    Value = $"{c.Id}"
                })
                .OrderBy(c => c.Text)
                .ToListAsync();

            list.Insert(0, new SelectListItem {
                Text = "[Seleccione una categoría...]",
                Value = "0"
            });

            return list;
        }

        public async Task<IEnumerable<SelectListItem>> GetComboCategoriesAsync(IEnumerable<Category> filter) {
            var categories = await _context.Categories
                .ToListAsync();

            var categoriesFilteren = new List<Category>();

            foreach (var category in categories) {
                if (!filter.Any(c => c.Id == category.Id)) {
                    categoriesFilteren.Add(category);
                }
            }

            List<SelectListItem> list = categoriesFilteren
                .Select(c => new SelectListItem {
                    Text = c.Name,
                    Value = $"{c.Id}"
                })
                .OrderBy(c => c.Text)
                .ToList();

            list.Insert(0, new SelectListItem {
                Text = "[Seleccione una categoría...]",
                Value = "0"
            });

            return list;
        }

        public async Task<IEnumerable<SelectListItem>> GetComboCitiesAsync(int stateId) {
            List<SelectListItem> list = await _context.Cities
                .Where(c => c.State.Id == stateId)
                .Select(c => new SelectListItem {
                    Text = c.Name,
                    Value = $"{c.Id}"
                })
                .OrderBy(c => c.Text)
                .ToListAsync();

            list.Insert(0, new SelectListItem {
                Text = "[Seleccione una ciudad...]",
                Value = "0"
            });

            return list;
        }

        public async Task<IEnumerable<SelectListItem>> GetComboCountriesAsync() {
            List<SelectListItem> list = await _context.Countries
                .Select(x => new SelectListItem {
                    Text = x.Name,
                    Value = $"{x.Id}"
                })
                .OrderBy(x => x.Text)
                .ToListAsync();

            list.Insert(0, new SelectListItem {
                Text = "[Seleccione un país...]",
                Value = "0"
            });

            return list;
        }

        public async Task<IEnumerable<SelectListItem>> GetComboStatesAsync(int countryId) {
            List<SelectListItem> list = await _context.States
                .Where(s => s.Country.Id == countryId)
                .Select(s => new SelectListItem {
                    Text = s.Name,
                    Value = $"{s.Id}"
                })
                .OrderBy(s => s.Text)
                .ToListAsync();

            list.Insert(0, new SelectListItem {
                Text = "[Seleccione un estado...]",
                Value = "0"
            });

            return list;
        }
    }
}