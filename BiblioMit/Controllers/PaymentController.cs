using BiblioMit.Data;
using BiblioMit.Models;
using BiblioMit.Models.Entities.Ads;
using BiblioMit.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace BiblioMit.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IFlow _flow;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public PaymentController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IFlow flow)
        {
            _userManager = userManager;
            _flow = flow;
            _context = context;
        }
        [HttpGet]
        public IActionResult Index()
        {
            var model = new Payment
            {
                Id = 0,
                Price = 100000,
                PeriodDate = DateTime.Now
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int Id)
        {
            var payment = new Payment();
            var email = string.Empty;
            if(Id == 0)
            {
                payment.Id = DateTime.Now.Millisecond;
                payment.Price = 100000;
                var user = await _userManager.FindByNameAsync(User.Identity.Name).ConfigureAwait(false);
                email = user.Email;
            }
            else
            {
                payment = await _context.Payments
                    .Include(p => p.Banner)
                        .ThenInclude(b => b.ApplicationUser)
                    .FirstOrDefaultAsync(p => p.Id == Id).ConfigureAwait(false);
                email = payment.Banner.ApplicationUser.Email;
            }
            var url = _flow.PaymentCreate(
                payment.Id, "PagoParticular",
                payment.Price,
                email);
            return Redirect(url);
        }
    }
}
