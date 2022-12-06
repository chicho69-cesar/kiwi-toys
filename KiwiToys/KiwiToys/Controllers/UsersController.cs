using KiwiToys.Data.Entities;
using KiwiToys.Data;
using KiwiToys.Enums;
using KiwiToys.Helpers;
using KiwiToys.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Vereyon.Web;
using KiwiToys.Common;
using Microsoft.EntityFrameworkCore;

namespace KiwiToys.Controllers {
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller {
        private readonly IUserHelper _userHelper;
        private readonly DataContext _context;
        private readonly ICombosHelper _combosHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly IGetLocation _getLocation;
        private readonly IMailHelper _mailHelper;
        private readonly IFlashMessage _flashMessage;
        private readonly IBodyMailHelper _bodyMailHelper;

        public UsersController(
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
        public async Task<IActionResult> Index() {
            var users = await _context.Users
                .Include(u => u.City)
                .ThenInclude(c => c.State)
                .ThenInclude(s => s.Country)
                .ToListAsync();

            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Create() {
            var model = new AddUserViewModel {
                Id = Guid.Empty.ToString(),
                Countries = await _combosHelper.GetComboCountriesAsync(),
                States = await _combosHelper.GetComboStatesAsync(0),
                Cities = await _combosHelper.GetComboCitiesAsync(0),
                UserType = UserType.Admin,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddUserViewModel model) {
            if (ModelState.IsValid) {
                Guid imageId = Guid.Empty;

                if (model.ImageFile != null) {
                    imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "users");
                }

                model.ImageId = imageId;
                User user = await _userHelper.AddUserAsync(model);

                if (user == null) {
                    _flashMessage.Danger("Este correo ya está siendo usado, o la contraseña es incorrecta");

                    model.Countries = await _combosHelper.GetComboCountriesAsync();
                    model.States = await _combosHelper.GetComboStatesAsync(0);
                    model.Cities = await _combosHelper.GetComboCitiesAsync(0);

                    return View(model);
                }

                model.Countries = await _combosHelper.GetComboCountriesAsync();
                model.States = await _combosHelper.GetComboStatesAsync(0);
                model.Cities = await _combosHelper.GetComboCitiesAsync(0);

                string myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);

                string tokenLink = Url.Action("ConfirmEmail", "Accounts", new {
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
                    _flashMessage.Info("Las instrucciones para habilitar al administrador han sido enviadas al correo.");
                    return View(model);
                }

                _flashMessage.Danger(response.Message);
            }

            _flashMessage.Danger("La informacion no es correcta revisa otra vez");

            return View(model);
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