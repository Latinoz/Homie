using Homie.Areas.Battletech.Models;
using Homie.Data.Models;
using Homie.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Homie.Areas.Battletech.Controllers
{
    [Area("Battletech")]
    [Authorize(Roles = "admin,user,battletech")]
    public class PilotsController : Controller
    {
        ApplicationDbContext db;
        

        public PilotsController(ApplicationDbContext context)
        {
            db = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var test = db.BtPilotEF.Where(a => a.UserUid == userId);
                //.
                //Include(u => u.BTPilotsModel).ToList();

            var test2 = db.BtEF.Where(a => a.UserUid == userId);

            //List<BTPilotsModel> pilots = new List<BTPilotsModel>();           

            //foreach(BTMechsModel mech in db.BtEF.Where(a => a.UserUid == userId).
            //    Include(u => u.BTPilotsModel).ToList())
            //{
            //    pilots.Add(mech.BTPilotsModel);
            //}

            //return View(pilots);
            return null;
        }
    }
}
