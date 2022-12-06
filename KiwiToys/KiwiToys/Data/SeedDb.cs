using KiwiToys.Common.Responses;
using KiwiToys.Common;
using KiwiToys.Data.Entities;
using KiwiToys.Helpers;
using KiwiToys.Services;
using Microsoft.EntityFrameworkCore;
using KiwiToys.Enums;

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
            await CheckRolesAsync();
            await CheckUsersAsync();
            await CheckProductsAsync();
            await CheckNewsAsync();
            await CheckOpinionsAsync();
        }

        private async Task CheckCategoriesAsync() {
            if (!_context.Categories.Any()) {
                _context.Categories.Add(new Category { Name = "Tecnología" });
                _context.Categories.Add(new Category { Name = "Deportes" });
                _context.Categories.Add(new Category { Name = "Marvel" });
                _context.Categories.Add(new Category { Name = "DC" });
                _context.Categories.Add(new Category { Name = "Hasbro" });
                _context.Categories.Add(new Category { Name = "Ropa" });
                _context.Categories.Add(new Category { Name = "Gamer" });
                _context.Categories.Add(new Category { Name = "Storm" });
                _context.Categories.Add(new Category { Name = "BMX" });
                _context.Categories.Add(new Category { Name = "Calzado" });
                _context.Categories.Add(new Category { Name = "Coleccionables" });
                _context.Categories.Add(new Category { Name = "Comics" });
                _context.Categories.Add(new Category { Name = "Funkos" });

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

                country = new() {
                    Name = countryResponse.Name,
                    States = new List<State>()
                };

                Response responseStates = await _apiService
                    .GetListAsync<StateResponse>("/v1", $"/countries/{countryResponse.Iso2}/states");

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
                            .GetListAsync<CityResponse>("/v1", $"/countries/{countryResponse.Iso2}/states/{stateResponse.Iso2}/cities");

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

        private async Task CheckRolesAsync() {
            await _userHelper.CheckRoleAsync(UserType.Admin.ToString());
            await _userHelper.CheckRoleAsync(UserType.User.ToString());
        }

        private async Task CheckUsersAsync() {
            await AddUserAsyn("1010", "Cesar", "Villalobos Olmos", "cesar@yopmail.com", "3461099207", "Av. Cananal #9", "Cesar.jpg", UserType.Admin);
            await AddUserAsyn("2020", "Lizeth", "Sandoval Vallejo", "liz@yopmail.com", "3461005286", "Calle de la Virgen #47", "Licha.jpg", UserType.Admin);
            await AddUserAsyn("1010", "Lucy", "Macias Martinez", "lucy@yopmail.com", "4952361232", "Calle Negrete #467", "Lucy.jpg", UserType.User);
            await AddUserAsyn("2020", "Joss", "Martinez Acosta", "joss@yopmail.com", "4952365217", "Av. Boulevar #4756", "Joss.jpg", UserType.User);
        }

        private async Task<User> AddUserAsyn(
            string document,
            string firstName,
            string lastName,
            string email,
            string phone,
            string address,
            string image,
            UserType userType
        ) {
            User user = await _userHelper.GetUserAsync(email);

            if (user is null) {
                Guid imageId = await _blobHelper.UploadBlobAsync($"{Environment.CurrentDirectory}\\wwwroot\\images\\users\\{image}", "users");

                user = new User {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    UserName = email,
                    PhoneNumber = phone,
                    Address = address,
                    Document = document,
                    City = _context.Cities.FirstOrDefault(),
                    UserType = userType,
                    ImageId = imageId
                };

                await _userHelper.AddUserAsync(user, "abc-123-ABC");
                await _userHelper.AddUserToRoleAsync(user, userType.ToString());

                string token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                await _userHelper.ConfirmEmailAsync(user, token);
            }

            return user;
        }

        private async Task CheckProductsAsync() {
            if (!_context.Products.Any()) {
                await AddProductAsync("Figura coleccionable de Godzilla", 699M, 10F, new List<string>() { "Hasbro", "Storm" }, new List<string>() { "prod1.png" }, new List<string>() { });
                await AddProductAsync("Funko Pikachu, Kimetsu No Yaiba", 1229M, 34F, new List<string>() { "Hasbro", "Funkos" }, new List<string>() { "prod2.png" }, new List<string>() { });
                await AddProductAsync("Figura de Acción Batman", 259M, 12F, new List<string>() { "DC" }, new List<string>() { "prod3.png" }, new List<string>() { });
                await AddProductAsync("Funko Batman", 499M, 23F, new List<string>() { "DC", "Hasbro", "Comics" }, new List<string>() { "prod4.png" }, new List<string>() { });
                await AddProductAsync("Funko Pop AMLO", 549M, 35F, new List<string>() { "Hasbro", "Funkos" }, new List<string>() { "prod5.png" }, new List<string>() { "El mejor Funko que existe", "Le compre este Funko a mi sobrinito y le encanto, super, super padre la verdad" });
                await AddProductAsync("Figura de Acción Iron-Man", 1299M, 7F, new List<string>() { "Marvel", "Comics", "Storm" }, new List<string>() { "prod6.png", "prod7.png" }, new List<string>() { "Esta espectacular esta figura de acción, la acabo de comprar y me encanto, super recomendada" });
                await AddProductAsync("Figura de Acción Frog-Man", 1119M, 5F, new List<string>() { "DC", "Comics" }, new List<string>() { "prod8.png" }, new List<string>() { });
                await AddProductAsync("Figura de Acción Baron Zemo", 1359M, 3F, new List<string>() { "Marvel", "Comics", "Storm" }, new List<string>() { "prod9.png" }, new List<string>() { });
                await AddProductAsync("Figura de Acción Wolverine", 1359M, 6F, new List<string>() { "Marvel", "Coleccionables", "Comics", "Hasbro" }, new List<string>() { "prod10.png" }, new List<string>() { });
                await AddProductAsync("Figura de Acción Moon Knight", 899M, 12F, new List<string>() { "Marvel" }, new List<string>() { "prod11.png" }, new List<string>() { });
                await AddProductAsync("Juguete Gran Robot", 799M, 15F, new List<string>() { "Coleccionables", "Hasbro" }, new List<string>() { "prod12.png" }, new List<string>() { });
                await AddProductAsync("Figura de Acción de Goku", 2759M, 35F, new List<string>() { "Coleccionables", "Storm" }, new List<string>() { "prod13.png" }, new List<string>() { });
                await AddProductAsync("Adidas Barracuda", 2700M, 12F, new List<string>() { "Calzado" }, new List<string>() { "adidas_barracuda.png" }, new List<string>() { });
                await AddProductAsync("Adidas Superstar", 2500M, 12F, new List<string>() { "Calzado" }, new List<string>() { "Adidas_superstar.png" }, new List<string>() {});
                await AddProductAsync("AirPods", 13000M, 12F, new List<string>() { "Tecnología" }, new List<string>() { "airpos.png", "airpos2.png" }, new List<string>() {});
                await AddProductAsync("Bicicleta Ribble", 120000M, 6F, new List<string>() { "BMX" }, new List<string>() { "bicicleta_ribble.png" }, new List<string>() {});
                await AddProductAsync("Camisa Cuadros", 560M, 24F, new List<string>() { "Ropa" }, new List<string>() { "camisa_cuadros.png" }, new List<string>() {});
                await AddProductAsync("Casco Bicicleta", 8200M, 12F, new List<string>() { "BMX" }, new List<string>() { "casco_bicicleta.png", "casco.png" }, new List<string>() {});
                await AddProductAsync("Mancuernas", 3700M, 12F, new List<string>() { "Deportes" }, new List<string>() { "mancuernas.png" }, new List<string>() {});
                await AddProductAsync("New Balance 530", 1800M, 12F, new List<string>() { "Calzado", "Deportes" }, new List<string>() { "newbalance530.png" }, new List<string>() {});
                await AddProductAsync("New Balance 565", 1790M, 12F, new List<string>() { "Calzado", "Deportes" }, new List<string>() { "newbalance565.png" }, new List<string>() {});
                await AddProductAsync("Nike Air", 2330M, 12F, new List<string>() { "Calzado", "Deportes" }, new List<string>() { "nike_air.png" }, new List<string>() { "Son los mejores tenis que he comprada, estan muy bonitos comodos y me han durado muchisimo, los recomiendo muchisimo la verdad" });
                await AddProductAsync("Nike Zoom", 2499M, 12F, new List<string>() { "Calzado", "Deportes" }, new List<string>() { "nike_zoom.png" }, new List<string>() {});
                await AddProductAsync("Buso Adidas Mujer", 1340M, 12F, new List<string>() { "Ropa", "Deportes" }, new List<string>() { "buso_adidas.png" }, new List<string>() {});
                await AddProductAsync("Teclado Gamer", 670M, 12F, new List<string>() { "Gamer", "Tecnología" }, new List<string>() { "teclado_gamer.png" }, new List<string>() {});
                await AddProductAsync("Silla Gamer", 9800M, 12F, new List<string>() { "Gamer", "Tecnología" }, new List<string>() { "silla_gamer.png" }, new List<string>() { "Esperaba mas de esta silla, en las fotos que se ve bonita pero no me gusto en general" });
                await AddProductAsync("Mouse Gamer", 1320M, 12F, new List<string>() { "Gamer", "Tecnología" }, new List<string>() { "mouse_gamer.png" }, new List<string>() {});
                
                await _context.SaveChangesAsync();
            }
        }

        private async Task AddProductAsync(string name, decimal price, float stock, List<string> categories, List<string> images, List<string> comments) {
            var product = new Product {
                Description = name,
                Name = name,
                Price = price,
                Stock = stock,
                ProductCategories = new List<ProductCategory>(),
                ProductImages = new List<ProductImage>(),
                Comments = new List<Comment>()
            };

            foreach (string category in categories) {
                product.ProductCategories
                    .Add(new ProductCategory {
                        Category = await _context.Categories
                        .FirstOrDefaultAsync(c => c.Name == category)
                    });
            }

            foreach (string image in images) {
                Guid imageId = await _blobHelper.UploadBlobAsync($"{Environment.CurrentDirectory}\\wwwroot\\images\\products\\{image}", "products");
                product.ProductImages
                    .Add(new ProductImage {
                        ImageId = imageId
                    });
            }

            foreach (string comment in comments) {
                product.Comments
                    .Add(new Comment {
                        User = await _context.Users.FirstOrDefaultAsync(),
                        Date = DateTime.Now,
                        Remark = comment
                    });
            }

            _context.Products.Add(product);
        }

        public async Task CheckNewsAsync() {
            await AddNewsAsync("Hasbro anuncia la figura de Spider-Man Japones", "El Emisario del Infierno rojo y azul obtiene su propia figura completa con Spider Bracelet y webs gracias a la serie Marvel Legends de Hasbro.\r\nLa versión japonesa de Spider-Man está obteniendo su propia figura como parte de la serie Marvel Legends de Hasbro.\r\n\r\n\r\nAhora disponible para pre-ordenar en el sitio web de Hasbro Pulse, el Spider-Man japonés, a veces denominado \"Supaidaman\", mide seis pulgadas y viene con múltiples accesorios, que incluyen manos intercambiables, telarañas y su marca registrada Spider Bracelet. La figura también cuenta con varios puntos de articulación perfectos para recrear las poses de arácnido salvaje de Spidey, algo común en el programa de televisión, donde el personaje adoptaba con frecuencia una postura dinámica mientras se anunciaba a sí mismo como \"el Emisario del Infierno, Spider-Man\".", "noticia1.jpg");
            await AddNewsAsync("Tianguisando", "Tianguis es el mercado tradicional que ha existido desde la época prehispánica y que ha ido evolucionando en forma y contexto social a lo largo de los siglos, el termino Tianguisar o Tianguisando hace referencia al ir a pasear y disfrutar de ver y comprar cosas en el tianguis.", "noticia2.jpg");

            await _context.SaveChangesAsync();
        }

        public async Task AddNewsAsync(string title, string description, string image) {
            if (await _context.News.AnyAsync(n => n.Title == title)) {
                return;
            }

            Guid imageId = await _blobHelper.UploadBlobAsync($"{Environment.CurrentDirectory}\\wwwroot\\images\\news\\{image}", "news");

            var news = new News {
                User = await _context.Users.FirstOrDefaultAsync(),
                Date = DateTime.Now,
                Title = title,
                Description = description,
                ImageId = imageId
            };

            _context.News.Add(news);
        }

        public async Task CheckOpinionsAsync() {
            await AddOpinionAsync("Esta increible como les quedo la pagina web, el diseño es bastante atractivo, es facil de usar y la antencion super buena");
            await AddOpinionAsync("Me encanta el diseño que implementaron, es bastante fresco e intuitivo");
            await AddOpinionAsync("Me gusto mucho porque los envios son eficientes y me llego mi pedido excatamente como lo pedí");
            await AddOpinionAsync("Le compre unas cosas a mi novia y se las enviaron envueltas en papel de regalo, definitivamente excelente servicio");
            await AddOpinionAsync("Las figuras que manejan son excelentes, de la mejor calidad y el precio es bastante razonable, definitivamente el mejor lugar para coleccionistas");
            
            await _context.SaveChangesAsync();
        }

        public async Task AddOpinionAsync(string remark) {
            if (await _context.Opinions.AnyAsync(o => o.Remark == remark)) {
                return;
            }

            var opinion = new Opinion {
                User = await _context.Users.FirstOrDefaultAsync(),
                Date = DateTime.Now,
                Remark = remark
            };

            _context.Opinions.Add(opinion);
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