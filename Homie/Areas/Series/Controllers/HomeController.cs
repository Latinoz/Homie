using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Homie.Areas.Series.Models;
using Microsoft.EntityFrameworkCore;
using Homies.Data.Models;
using Microsoft.AspNetCore.Authorization;

namespace Homie.Areas.Series.Controllers
{
    [Area("Series")]
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
            return View(await db.MoviesEF.Where(a => a.Archive == false).ToListAsync());
        }

        public async Task<IActionResult> ArchMovies()
        {
            return View(await db.MoviesEF.Where(a => a.Archive == true).ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(MoviesModel movie)
        {
            db.MoviesEF.Add(movie);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> GoToArchive(int? Id)
        {
            if (Id != null)
            {
                MoviesModel movie = await db.MoviesEF.FirstOrDefaultAsync(p => p.Id == Id);
                movie.Archive = true;                

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }            
            return NotFound();
        }

        public async Task<IActionResult> BackToList(int? Id)
        {
            if (Id != null)
            {
                MoviesModel movie = await db.MoviesEF.FirstOrDefaultAsync(p => p.Id == Id);
                movie.Archive = false;

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return NotFound();
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id != null)
            {
                MoviesModel movie = await db.MoviesEF.FirstOrDefaultAsync(p => p.Id == id);
                if (movie != null)
                    return View(movie);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(MoviesModel movie)
        {
            db.MoviesEF.Update(movie);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int? id)
        {
            if (id != null)
            {
                MoviesModel movie = await db.MoviesEF.FirstOrDefaultAsync(p => p.Id == id);
                if (movie != null)
                    return View(movie);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id != null)
            {
                MoviesModel movie = await db.MoviesEF.FirstOrDefaultAsync(p => p.Id == Id);
                if (movie != null)
                {
                    db.MoviesEF.Remove(movie);
                    await db.SaveChangesAsync();

                    if(movie.Archive == false)
                    {
                        return RedirectToAction("Index");
                    }
                    return RedirectToAction("ArchMovies", "Home", new { area = "Series" });
                }
            }
            return NotFound();
        }
    }
}