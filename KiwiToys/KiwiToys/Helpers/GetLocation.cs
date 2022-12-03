using KiwiToys.Data.Entities;
using KiwiToys.Data;
using Microsoft.EntityFrameworkCore;

namespace KiwiToys.Helpers {
    public class GetLocation : IGetLocation {
        private readonly DataContext _context;

        public GetLocation(DataContext context) {
            _context = context;
        }

        public IOrderedEnumerable<State> GetStates(int countryId) {
            Country country = _context.Countries
                .Include(c => c.States)
                .FirstOrDefault(c => c.Id == countryId);

            if (country == null) {
                return null;
            }

            return country.States
                .OrderBy(d => d.Name);
        }

        public IOrderedEnumerable<City> GetCities(int stateId) {
            State state = _context.States
                .Include(s => s.Cities)
                .FirstOrDefault(s => s.Id == stateId);

            if (state == null) {
                return null;
            }

            return state.Cities
                .OrderBy(c => c.Name);
        }
    }
}