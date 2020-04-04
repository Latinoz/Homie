using System;
using System.Collections.Generic;
using Homie.Areas.Cigars.Models;
using System.Linq;
using System.Threading.Tasks;
using Homies.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Homie.Areas.Cigars.Controllers
{
    [Area("Cigars")]
    [Authorize(Roles = "admin,user")]
    public class HomeController : Controller
    {
        ApplicationDbContext db;

        public HomeController(ApplicationDbContext context)
        {
            db = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await db.CigarsEF.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CigarsModel cigar)
        {
            db.CigarsEF.Add(cigar);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }        

        public async Task<IActionResult> Edit(int? id)
        {
            if (id != null)
            {
                CigarsModel cigar = await db.CigarsEF.FirstOrDefaultAsync(p => p.Id == id);
                if (cigar != null)
                    return View(cigar);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CigarsModel cigar)
        {
            db.CigarsEF.Update(cigar);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int? id)
        {
            if (id != null)
            {
                CigarsModel cigar = await db.CigarsEF.FirstOrDefaultAsync(p => p.Id == id);
                if (cigar != null)
                    return View(cigar);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id != null)
            {
                CigarsModel cigar = await db.CigarsEF.FirstOrDefaultAsync(p => p.Id == Id);
                if (cigar != null)
                {
                    db.CigarsEF.Remove(cigar);
                    await db.SaveChangesAsync();

                    return RedirectToAction("Index");
                    
                }
            }
            return NotFound();
        }
    }
}