using KiwiToys.Data;
using KiwiToys.Data.Entities;
using KiwiToys.Helpers;
using KiwiToys.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KiwiToys.Controllers {
    public class CommentsController : Controller {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public CommentsController(DataContext context, IUserHelper userHelper) {
            _context = context;
            _userHelper = userHelper;
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(AddCommentViewModel model) {
            User user = await _userHelper.GetUserAsync(User.Identity.Name);

            if (user == null) {
                return NotFound();
            }

            Product product = await _context.Products
                .Where(p => p.Id == model.ProductId)
                .FirstOrDefaultAsync();

            var comment = new Comment {
                User = user,
                Product = product,
                Date = DateTime.Now,
                Remark = model.Remark
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Home", new { id = model.ProductId });
        }
    }
}