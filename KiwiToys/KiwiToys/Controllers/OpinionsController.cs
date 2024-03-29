﻿using KiwiToys.Data;
using KiwiToys.Data.Entities;
using KiwiToys.Helpers;
using KiwiToys.Models.Comments;
using Microsoft.AspNetCore.Mvc;

namespace KiwiToys.Controllers {
    public class OpinionsController : Controller {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public OpinionsController(DataContext context, IUserHelper userHelper) {
            _context = context;
            _userHelper = userHelper;
        }

        [HttpGet]
        public IActionResult Index() {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddOpinion(AddOpinionViewModel model) {
            User user = await _userHelper.GetUserAsync(User.Identity.Name);

            if (user == null) {
                return NotFound();
            }

            var opinion = new Opinion {
                User = user,
                Date = DateTime.Now,
                Remark = model.Remark
            };

            _context.Opinions.Add(opinion);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}