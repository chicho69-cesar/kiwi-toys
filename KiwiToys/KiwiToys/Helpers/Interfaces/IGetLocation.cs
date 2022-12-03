using KiwiToys.Data.Entities;

namespace KiwiToys.Helpers {
    public interface IGetLocation {
        IOrderedEnumerable<State> GetStates(int countryId);
        IOrderedEnumerable<City> GetCities(int stateId);
    }
}