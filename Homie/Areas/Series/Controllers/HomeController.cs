using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Homies.Models;
using Homie.Areas.Series.Models;
using Microsoft.EntityFrameworkCore;

namespace Homie.Areas.Series.Controllers
{
    [Area("Series")]
    public class HomeController : Controller
    {
        ApplicationDbContext db;

        public HomeController(ApplicationDbContext context)
        {
            db = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await db.Movies.ToListAsync());
        }

        public async Task<IActionResult> ArchMovies()
        {
            return View(await db.ArchMovies.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Movies movie)
        {
            db.Movies.Add(movie);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id != null)
        //    {
        //        Movies user = await db.Movies.FirstOrDefaultAsync(p => p.Id == id);
        //        if (user != null)
        //            return View(user);
        //    }
        //    return NotFound();
        //}

        public async Task<IActionResult> Edit(int? id)
        {
            if (id != null)
            {
                Movies movie = await db.Movies.FirstOrDefaultAsync(p => p.Id == id);
                if (movie != null)
                    return View(movie);
            }
            return NotFound();
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Movies movie)
        {
            db.Movies.Update(movie);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int? id)
        {
            if (id != null)
            {
                Movies user = await db.Movies.FirstOrDefaultAsync(p => p.Id == id);
                if (user != null)
                    return View(user);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id != null)
            {
                Movies user = await db.Movies.FirstOrDefaultAsync(p => p.Id == id);
                if (user != null)
                {
                    db.Movies.Remove(user);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
            return NotFound();
        }
    }
}