using KiwiToys.Data.Entities;
using KiwiToys.Data;
using KiwiToys.Helpers;
using KiwiToys.Models;
using KiwiToys.Validations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Vereyon.Web;

namespace KiwiToys.Controllers {
    [Authorize(Roles = "Admin")]
    public class CountriesController : Controller {
        private readonly DataContext _context;
        private readonly IFlashMessage _flashMessage;

        public CountriesController(DataContext context, IFlashMessage flashMessage) {
            _context = context;
            _flashMessage = flashMessage;
        }

        [HttpGet]
        public async Task<IActionResult> Index() {
            var countries = await _context.Countries
                .Include(c => c.States)
                .ThenInclude(s => s.Cities)
                .ToListAsync();

            return View(countries);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id) {
            if (id == null || _context.Countries == null) {
                return NotFound();
            }

            var country = await _context.Countries
                .Include(c => c.States)
                .ThenInclude(s => s.Cities)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (country is null) {
                return NotFound();
            }

            return View(country);
        }

        [NoDirectAccess]
        public async Task<IActionResult> Delete(int? id) {
            if (id == null) {
                return NotFound();
            }

            Country country = await _context.Countries.FirstOrDefaultAsync(c => c.Id == id);

            if (country == null) {
                return NotFound();
            }

            try {
                _context.Countries.Remove(country);
                await _context.SaveChangesAsync();
                _flashMessage.Info("País borrado con exito");
            } catch {
                _flashMessage.Danger("No se puede borrar el país porque tiene registros relacionados.");
            }

            return RedirectToAction(nameof(Index));
        }

        [NoDirectAccess]
        public async Task<IActionResult> AddOrEdit(int id = 0) {
            if (id == 0) {
                return View(new Country());
            } else {
                Country country = await _context.Countries
                    .FindAsync(id);

                if (country == null) {
                    return NotFound();
                }

                return View(country);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(int id, Country country) {
            if (ModelState.IsValid) {
                try {
                    if (id == 0) {
                        _context.Add(country);
                        await _context.SaveChangesAsync();
                        _flashMessage.Info("Registro creado.");
                    } else {
                        _context.Update(country);
                        await _context.SaveChangesAsync();
                        _flashMessage.Info("Registro actualizado.");
                    }

                    return Json(new {
                        isValid = true,
                        html = ModalHelper.RenderRazorViewToString(
                            this,
                            "_ViewAllCountries",
                            _context.Countries
                                .Include(c => c.States)
                                .ThenInclude(s => s.Cities)
                                .ToList())
                    });
                } catch (DbUpdateException dbUpdateException) {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate")) {
                        _flashMessage.Danger("Ya existe un país con el mismo nombre.");
                    } else {
                        _flashMessage.Danger(dbUpdateException.InnerException.Message);
                    }
                } catch (Exception exception) {
                    _flashMessage.Danger(exception.Message);
                }
            }

            return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddOrEdit", country) });
        }

        [HttpGet]
        public async Task<IActionResult> DetailsState(int? id) {
            if (id == null) {
                return NotFound();
            }

            var state = await _context.States
                .Include(s => s.Country)
                .Include(s => s.Cities)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (state is null) {
                return NotFound();
            }

            return View(state);
        }

        [NoDirectAccess]
        public async Task<IActionResult> AddState(int id) {
            var country = await _context.Countries.FindAsync(id);

            if (country is null) {
                return NotFound();
            }

            var model = new StateViewModel {
                CountryId = country.Id
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddState(StateViewModel model) {
            if (ModelState.IsValid) {
                try {
                    var state = new State {
                        Cities = new List<City>(),
                        Country = await _context.Countries.FindAsync(model.CountryId),
                        Name = model.Name
                    };

                    _context.Add(state);
                    await _context.SaveChangesAsync();

                    Country country = await _context.Countries
                        .Include(c => c.States)
                        .ThenInclude(s => s.Cities)
                        .FirstOrDefaultAsync(c => c.Id == model.CountryId);

                    _flashMessage.Info("Estado creado correctamente");

                    return Json(
                        new {
                            isValid = true,
                            html = ModalHelper.RenderRazorViewToString(this, "_ViewAllStates", country)
                        }
                    );
                } catch (DbUpdateException dbUpdateException) {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate")) {
                        _flashMessage.Danger("Ya existe un estado con el mismo nombre");
                    } else {
                        _flashMessage.Danger(dbUpdateException.InnerException.Message);
                    }
                } catch (Exception exception) {
                    _flashMessage.Danger(exception.Message);
                }
            }

            return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddState", model) });
        }

        [NoDirectAccess]
        public async Task<IActionResult> EditState(int? id) {
            if (id == null) {
                return NotFound();
            }

            var state = await _context.States
                .Include(s => s.Country)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (state is null) {
                return NotFound();
            }

            var model = new StateViewModel {
                CountryId = state.Country.Id,
                Id = state.Id,
                Name = state.Name
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditState(int id, StateViewModel model) {
            if (id != model.Id) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    var state = new State {
                        Id = model.Id,
                        Name = model.Name
                    };

                    _context.Update(state);

                    Country country = await _context.Countries
                        .Include(c => c.States)
                        .ThenInclude(s => s.Cities)
                        .FirstOrDefaultAsync(c => c.Id == model.CountryId);

                    await _context.SaveChangesAsync();

                    return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllStates", country) });
                } catch (DbUpdateException dbUpdateException) {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate")) {
                        _flashMessage.Danger("Ya existe un estado con el mismo nombre");
                    } else {
                        _flashMessage.Danger(dbUpdateException.InnerException.Message);
                    }
                } catch (Exception exception) {
                    _flashMessage.Danger(exception.Message);
                }
            }

            return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "EditState", model) });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteState(int? id) {
            if (id == null) {
                return NotFound();
            }

            State state = await _context.States
                .Include(s => s.Country)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (state == null) {
                return NotFound();
            }

            try {
                _context.States.Remove(state);
                await _context.SaveChangesAsync();
                _flashMessage.Info("Estado borrado correctamente");
            } catch {
                _flashMessage.Danger("No se puede borrar el estado porque tiene registros relacionados.");
            }

            return RedirectToAction(nameof(Details), new { Id = state.Country.Id });
        }

        [NoDirectAccess]
        public async Task<IActionResult> AddCity(int? id) {
            if (id == null) {
                return NotFound();
            }

            var state = await _context.States.FindAsync(id);

            if (state is null) {
                return NotFound();
            }

            var model = new CityViewModel {
                StateId = state.Id
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCity(CityViewModel model) {
            if (ModelState.IsValid) {
                State state = await _context.States
                    .FindAsync(model.StateId);

                City city = new() {
                    State = state,
                    Name = model.Name
                };

                _context.Add(city);

                try {
                    await _context.SaveChangesAsync();

                    state = await _context.States
                        .Include(s => s.Cities)
                        .FirstOrDefaultAsync(c => c.Id == model.StateId);

                    _flashMessage.Info("Ciudad agregada con exito");

                    return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllCities", state) });
                } catch (DbUpdateException dbUpdateException) {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate")) {
                        _flashMessage.Danger("Ya existe una ciudad con el mismo nombre.");
                    } else {
                        _flashMessage.Danger(dbUpdateException.InnerException.Message);
                    }
                } catch (Exception exception) {
                    _flashMessage.Danger(exception.Message);
                }
            }

            return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddCity", model) });
        }

        [NoDirectAccess]
        public async Task<IActionResult> EditCity(int? id) {
            if (id == null) {
                return NotFound();
            }

            var city = await _context.Cities
                .Include(c => c.State)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (city is null) {
                return NotFound();
            }

            var model = new CityViewModel {
                StateId = city.State.Id,
                Id = city.Id,
                Name = city.Name
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCity(int id, CityViewModel model) {
            if (id != model.Id) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    var city = new City {
                        Id = model.Id,
                        Name = model.Name,
                    };

                    _context.Update(city);
                    await _context.SaveChangesAsync();

                    State state = await _context.States
                        .Include(s => s.Cities)
                        .FirstOrDefaultAsync(c => c.Id == model.StateId);

                    _flashMessage.Confirmation("Ciudad actualizada");

                    return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAllCities", state) });
                } catch (DbUpdateException dbUpdateException) {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate")) {
                        _flashMessage.Danger("Ya existe una ciudad con el mismo nombre");
                    } else {
                        _flashMessage.Danger(dbUpdateException.InnerException.Message);
                    }
                } catch (Exception exception) {
                    _flashMessage.Danger(exception.Message);
                }
            }

            return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "EditCity", model) });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteCity(int? id) {
            if (id == null) {
                return NotFound();
            }

            City city = await _context.Cities
                .Include(c => c.State)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (city == null) {
                return NotFound();
            }

            try {
                _context.Cities.Remove(city);
                await _context.SaveChangesAsync();
            } catch {
                _flashMessage.Danger("No se puede borrar la ciudad porque tiene registros relacionados.");
            }

            _flashMessage.Info("Ciudad borrada con exito");

            return RedirectToAction(nameof(DetailsState), new { Id = city.State.Id });
        }
    }
}