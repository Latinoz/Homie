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

            List<BTPilotsModel> pilots = new List<BTPilotsModel>();

            pilots = db.BtPilotEF.Where(a => a.UserUid == userId)
                .Include(u => u.BTMechsModel).ToList();

            return View(pilots);            
        }

        [HttpGet]
        public IActionResult Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            BTPilotsModel model = new BTPilotsModel();

            List<BTMechsModel> mechs = new List<BTMechsModel>();

            mechs = db.BtEF.Where(a => a.UserUid == userId && a.BTPilotsModelId == null).ToList();

            ViewBag.ListofMechs = mechs;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(BTPilotsModel pilot)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            pilot.UserUid = userId;

            BTMechsModel mech = db.BtEF.FirstOrDefault(b => b.Id == pilot.MechId);           

            db.BtPilotEF.Add(pilot);
            db.SaveChanges();            

            mech.BTPilotsModelId = pilot.Id;

            db.BtEF.Update(mech);

            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
