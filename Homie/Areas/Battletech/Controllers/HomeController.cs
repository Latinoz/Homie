using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Homies.Data.Models;
using Homie.Areas.Battletech.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Homie.Models;

namespace Homie.Areas.Battletech.Controllers
{
    [Area("Battletech")]
    [Authorize(Roles = "admin,user,battletech")]
    public class HomeController : Controller
    {
        ApplicationDbContext db;

        public HomeController(ApplicationDbContext context)
        {
            db = context;
        }        

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    
            IQueryable<BTMechsModel> mechs = db.BTMechsEF.Where(a => a.UserUid == userId);

            IndexViewModel viewModel = new IndexViewModel
            {
                Mechs = mechs
            };
            return View(viewModel);
        }
    }
}