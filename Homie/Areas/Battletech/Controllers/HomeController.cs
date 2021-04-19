using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Homie.Data.Models;
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

        public IActionResult Create()
        {
           
            List<BTMechsModel> mechsModels = new List<BTMechsModel>();
            
            mechsModels = db.BTMechsEF.ToList();            

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(BTMechsModel mech)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
           
            mech.UserUid = userId;
           
            db.BTMechsEF.Add(mech);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int? id)
        {          

            if (id != null)
            {                
                BTMechsModel mech = await db.BTMechsEF.FirstOrDefaultAsync(p => p.Id == id);
                if (mech != null)
                    return View(mech);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(BTMechsModel mech)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            mech.UserUid = userId;         

            db.BTMechsEF.Update(mech);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int? id)
        {
            if (id != null)
            {                
                BTMechsModel mech = await db.BTMechsEF.FirstOrDefaultAsync(p => p.Id == id);
                  if (mech != null)
                return View(mech);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id != null)
            {                
                BTMechsModel mech = await db.BTMechsEF.FirstOrDefaultAsync(p => p.Id == Id);
                if (mech != null)
                {
                    db.BTMechsEF.Remove(mech);
                    await db.SaveChangesAsync();

                    return RedirectToAction("Index");

                }
            }
            return NotFound();
        }

    }
}