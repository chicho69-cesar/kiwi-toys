using KiwiToys.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KiwiToys.Helpers {
    public interface ICombosHelper {
        Task<IEnumerable<SelectListItem>> GetComboCategoriesAsync();
        Task<IEnumerable<SelectListItem>> GetComboCategoriesAsync(IEnumerable<Category> filter);
        Task<IEnumerable<SelectListItem>> GetComboCountriesAsync();
        Task<IEnumerable<SelectListItem>> GetComboStatesAsync(int countryId);
        Task<IEnumerable<SelectListItem>> GetComboCitiesAsync(int stateId);
    }
}