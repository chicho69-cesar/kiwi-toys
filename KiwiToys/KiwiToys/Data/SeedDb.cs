using KiwiToys.Common.Responses;
using KiwiToys.Common;
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
            await CheckCountries2Async();
        }

        private async Task CheckCategoriesAsync() {
            if (!_context.Categories.Any()) {
                _context.Categories.Add(new Category { Name = "Marvel" });
                _context.Categories.Add(new Category { Name = "DC" });
                _context.Categories.Add(new Category { Name = "Comics" });
                _context.Categories.Add(new Category { Name = "Figuras de accion" });
                _context.Categories.Add(new Category { Name = "Funkos" });
                _context.Categories.Add(new Category { Name = "Coleccionables" });
                _context.Categories.Add(new Category { Name = "Ropa" });

                await _context.SaveChangesAsync();
            }
        }

        private async Task CheckCountriesAsync() {
            if (_context.Countries.Any()) return;

            Response responseCountries = await _apiService
                .GetListAsync<CountryResponse>("/v1", "/countries");

            if (!responseCountries.IsSuccess) return;

            List<CountryResponse> countries = (List<CountryResponse>)responseCountries.Result;

            foreach (var countryResponse in countries) {
                Country country = await _context.Countries
                    .FirstOrDefaultAsync(c => c.Name == countryResponse.Name);

                if (country is not null) continue;

                country = new Country {
                    Name = countryResponse.Name,
                    States = new List<State>()
                };

                Response responseStates = await _apiService
                    .GetListAsync<StateResponse>("/v1", $"/countries/{ countryResponse.Iso2 }/states");

                if (responseStates.IsSuccess) {
                    List<StateResponse> states = (List<StateResponse>)responseStates.Result;

                    foreach (var stateResponse in states) {
                        State state = country.States
                            .FirstOrDefault(s => s.Name == stateResponse.Name);

                        if (state is not null) continue;

                        state = new() {
                            Name = stateResponse.Name,
                            Cities = new List<City>()
                        };

                        Response responseCities = await _apiService
                            .GetListAsync<CityResponse>("/v1", $"/countries/{ countryResponse.Iso2 }/states/{ stateResponse.Iso2 }/cities");

                        if (responseCities.IsSuccess) {
                            List<CityResponse> cities = (List<CityResponse>)responseCities.Result;

                            foreach (CityResponse cityResponse in cities) {
                                if (cityResponse.Name == "Mosfellsbær" || cityResponse.Name == "Șăulița") {
                                    continue;
                                }

                                City city = state.Cities
                                    .FirstOrDefault(c => c.Name == cityResponse.Name);

                                if (city is null) {
                                    state.Cities.Add(new City() {
                                        Name = cityResponse.Name
                                    });
                                }
                            }
                        }

                        if (state.CitiesNumber > 0) {
                            country.States.Add(state);
                        }
                    }
                }

                if (country.CitiesNumber > 0) {
                    await _context.Countries.AddAsync(country);
                    await _context.SaveChangesAsync();
                }
            }
        }




















        private async Task CheckCountries2Async() {
            if (!_context.Countries.Any()) {
                _context.Countries.Add(new Country { 
                    Name = "Colombia", 
                    States = new List<State>() { 
                        new State() { 
                            Name = "Antioquia", 
                            Cities = new List<City>() { 
                                new City() { Name = "Medellín" }, 
                                new City() { Name = "Itagüí" }, 
                                new City() { Name = "Envigado" }, 
                                new City() { Name = "Bello" }, 
                                new City() { Name = "Rionegro" }, 
                            } 
                        }, 
                        new State() { 
                            Name = "Bogotá", 
                            Cities = new List<City>() { 
                                new City() { Name = "Usaquen" }, 
                                new City() { Name = "Champinero" }, 
                                new City() { Name = "Santa fe" }, 
                                new City() { Name = "Useme" }, 
                                new City() { Name = "Bosa" }, 
                            } 
                        }, 
                    } 
                }); 
                
                _context.Countries.Add(new Country {
                    Name = "Estados Unidos",
                    States = new List<State>() {
                        new State() {
                            Name = "Florida",
                            Cities = new List<City>() {
                                new City() { Name = "Orlando" },
                                new City() { Name = "Miami" },
                                new City() { Name = "Tampa" },
                                new City() { Name = "Fort Lauderdale" },
                                new City() { Name = "Key West" },
                            }
                        },
                        new State() {
                            Name = "Texas",
                            Cities = new List<City>() {
                                new City() { Name = "Houston" },
                                new City() { Name = "San Antonio" },
                                new City() { Name = "Dallas" },
                                new City() { Name = "Austin" },
                                new City() { Name = "El Paso" },
                            }
                        },
                    }
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}