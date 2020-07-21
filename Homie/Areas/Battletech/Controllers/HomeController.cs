using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Homies.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Homie.Models;

namespace Homie.Areas.Battletech.Controllers
{
    [Area("Battletech")]
    [Authorize(Roles = "admin,user")]
    public class HomeController : Controller
    {
        ApplicationDbContext db;

        public HomeController(ApplicationDbContext context)
        {
            db = context;
        }        

        public IActionResult Index()
        {
            return View();
        }
    }
}