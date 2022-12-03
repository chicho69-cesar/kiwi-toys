using KiwiToys.Common;
using KiwiToys.Data;
using KiwiToys.Data.Entities;
using KiwiToys.Enums;
using KiwiToys.Helpers;
using KiwiToys.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace KiwiToys.Controllers {
    public class AccountsController : Controller {
        private readonly IUserHelper _userHelper;
        private readonly DataContext _context;
        private readonly ICombosHelper _combosHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly IGetLocation _getLocation;
        private readonly IMailHelper _mailHelper;
        private readonly IFlashMessage _flashMessage;
        private readonly IBodyMailHelper _bodyMailHelper;

        public AccountsController(
            IUserHelper userHelper,
            DataContext context,
            ICombosHelper combosHelper,
            IBlobHelper blobHelper,
            IGetLocation getLocation,
            IMailHelper mailHelper,
            IFlashMessage flashMessage,
            IBodyMailHelper bodyMailHelper
        ) {
            _userHelper = userHelper;
            _context = context;
            _combosHelper = combosHelper;
            _blobHelper = blobHelper;
            _getLocation = getLocation;
            _mailHelper = mailHelper;
            _flashMessage = flashMessage;
            _bodyMailHelper = bodyMailHelper;
        }

        [HttpGet]
        public IActionResult Login() {
            if (User.Identity.IsAuthenticated) {
                return RedirectToAction("Index", "Home");
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model) {
            if (ModelState.IsValid) {
                Microsoft.AspNetCore.Identity.SignInResult result = await _userHelper.LoginAsync(model);

                if (result.Succeeded) {
                    return RedirectToAction("Index", "Home");
                }

                if (result.IsLockedOut) {
                    _flashMessage.Danger("Ha superado el máximo número de intentos, su cuenta está bloqueada, intente de nuevo en 5 minutos.");
                } else if (result.IsNotAllowed) {
                    _flashMessage.Danger("El usuario no ha sido habilitado, debes de seguir las instrucciones enviadas al correo para poder habilitarlo.");
                } else {
                    _flashMessage.Danger("Email o contraseña incorrectos.");
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout() {
            await _userHelper.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Register() {
            var model = new AddUserViewModel {
                Id = Guid.Empty.ToString(),
                Countries = await _combosHelper.GetComboCountriesAsync(),
                States = await _combosHelper.GetComboStatesAsync(0),
                Cities = await _combosHelper.GetComboCitiesAsync(0),
                UserType = UserType.User,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(AddUserViewModel model) {
            if (ModelState.IsValid) {
                Guid imageId = Guid.Empty;

                if (model.ImageFile != null) {
                    imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "users");
                }

                model.ImageId = imageId;
                User user = await _userHelper.AddUserAsync(model);

                if (user == null) {
                    _flashMessage.Danger("Este correo ya está siendo usado, o la contraseña no es valida.");

                    model.Countries = await _combosHelper.GetComboCountriesAsync();
                    model.States = await _combosHelper.GetComboStatesAsync(0);
                    model.Cities = await _combosHelper.GetComboCitiesAsync(0);

                    return View(model);
                }

                string myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);

                string tokenLink = Url.Action("ConfirmEmail", "Account", new {
                    userid = user.Id,
                    token = myToken
                }, protocol: HttpContext.Request.Scheme);

                Response response = _mailHelper.SendMail(
                    $"{model.FirstName} {model.LastName}",
                    model.Username,
                    "Shopping Car - Confirmación de Email",
                    _bodyMailHelper.GetConfirmEmailMessage(tokenLink)
                );

                if (response.IsSuccess) {
                    _flashMessage.Info("Usuario registrado. Para poder ingresar al sistema, siga las instrucciones que han sido enviadas a su correo.");
                    return RedirectToAction(nameof(Login));
                }

                _flashMessage.Danger(response.Message);
            }

            model.Countries = await _combosHelper.GetComboCountriesAsync();
            model.States = await _combosHelper.GetComboStatesAsync(0);
            model.Cities = await _combosHelper.GetComboCitiesAsync(0);

            _flashMessage.Danger("La informacion que proporsionaste no es valida vuelve a intentarlo");

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token) {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token)) {
                return NotFound();
            }

            User user = await _userHelper.GetUserAsync(new Guid(userId));
            if (user == null) {
                return NotFound();
            }

            IdentityResult result = await _userHelper.ConfirmEmailAsync(user, token);
            if (!result.Succeeded) {
                return NotFound();
            }

            return View();
        }

        [HttpGet]
        public IActionResult RecoverPassword() {
            ViewBag.Message = "";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RecoverPassword(RecoverPasswordViewModel model) {
            if (ModelState.IsValid) {
                User user = await _userHelper.GetUserAsync(model.Email);

                if (user == null) {
                    _flashMessage.Danger("El email no corresponde a ningún usuario registrado.");
                    return View(model);
                }

                string myToken = await _userHelper.GeneratePasswordResetTokenAsync(user);
                string link = Url.Action(
                    "ResetPassword",
                    "Account",
                    new { token = myToken },
                    protocol: HttpContext.Request.Scheme
                );

                _mailHelper.SendMail(
                    $"{user.FullName}",
                    model.Email,
                    "Shopping - Recuperación de Contraseña",
                    _bodyMailHelper.GetResetPasswordMessage(link)
                );

                _flashMessage.Info("Las instrucciones para recuperar la contraseña han sido enviadas a su correo.");

                return RedirectToAction(nameof(Login));
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string token) {
            ViewBag.Message = "";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model) {
            User user = await _userHelper.GetUserAsync(model.UserName);

            if (user != null) {
                IdentityResult result = await _userHelper.ResetPasswordAsync(user, model.Token, model.Password);

                if (result.Succeeded) {
                    _flashMessage.Info("Contraseña cambiada con éxito.");
                    return RedirectToAction(nameof(Login));
                }

                _flashMessage.Danger("Error cambiando la contraseña.");
                return View(model);
            }

            _flashMessage.Danger("Usuario no encontrado.");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ChangeUser() {
            User user = await _userHelper.GetUserAsync(User.Identity.Name);

            if (user == null) {
                return NotFound();
            }

            var model = new EditUserViewModel {
                Address = user.Address,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                ImageId = user.ImageId,
                Cities = await _combosHelper.GetComboCitiesAsync(user.City.State.Id),
                CityId = user.City.Id,
                Countries = await _combosHelper.GetComboCountriesAsync(),
                CountryId = user.City.State.Country.Id,
                StateId = user.City.State.Id,
                States = await _combosHelper.GetComboStatesAsync(user.City.State.Country.Id),
                Id = user.Id,
                Document = user.Document
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUser(EditUserViewModel model) {
            if (ModelState.IsValid) {
                Guid imageId = model.ImageId;

                if (model.ImageFile != null) {
                    imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "users");
                }

                User user = await _userHelper.GetUserAsync(User.Identity.Name);

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Address = model.Address;
                user.PhoneNumber = model.PhoneNumber;
                user.ImageId = imageId;
                user.City = await _context.Cities.FindAsync(model.CityId);
                user.Document = model.Document;

                await _userHelper.UpdateUserAsync(user);

                return RedirectToAction("Index", "Home");
            }

            model.Countries = await _combosHelper.GetComboCountriesAsync();
            model.States = await _combosHelper.GetComboStatesAsync(0);
            model.Cities = await _combosHelper.GetComboCitiesAsync(0);

            return View(model);
        }

        [HttpGet]
        public IActionResult ChangePassword() {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model) {
            if (ModelState.IsValid) {
                if (model.OldPassword == model.NewPassword) {
                    _flashMessage.Danger("La nueva contraseña es igual a la contraseña actual");
                    return View(model);
                }

                var user = await _userHelper.GetUserAsync(User.Identity.Name);

                if (user != null) {
                    var result = await _userHelper.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

                    if (result.Succeeded) {
                        return RedirectToAction("ChangeUser");
                    } else {
                        _flashMessage.Danger("Contraseña incorrecta");
                    }
                } else {
                    _flashMessage.Danger("Usuario no encontrado");
                }
            }

            return View(model);
        }

        /* TODO: Descubrir para que mierda es esto*/
        [HttpGet]
        public IActionResult ResendToken() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendToken(ResendTokenViewModel model) {
            if (ModelState.IsValid) {
                User user = await _userHelper.GetUserAsync(model.Username);

                string myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                string tokenLink = Url.Action("ConfirmEmail", "Account", new {
                    userid = user.Id,
                    token = myToken
                }, protocol: HttpContext.Request.Scheme);

                Response response = _mailHelper.SendMail(
                    $"{model.FirstName} {model.LastName}",
                    model.Username,
                    "Shopping - Confirmación de Email",
                    _bodyMailHelper.GetConfirmEmailMessage(tokenLink)
                );

                if (response.IsSuccess) {
                    _flashMessage.Info("Email Re-Envíado. Para poder ingresar al sistema, siga las instrucciones que han sido enviadas a su correo.");
                    return RedirectToAction(nameof(Login));
                }

                _flashMessage.Danger(response.Message);
            }

            _flashMessage.Danger("Ocurrio un error con la información proporcionada");

            return View(model);
        }

        [HttpGet]
        public IActionResult NotAuthorized() {
            return View();
        }

        public dynamic GetViewBag() {
            return ViewBag;
        }

        public JsonResult GetStates(int countryId) {
            var states = _getLocation.GetStates(countryId);
            return Json(states);
        }

        public JsonResult GetCities(int stateId) {
            var states = _getLocation.GetCities(stateId);
            return Json(states);
        }
    }
}