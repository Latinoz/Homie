using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Homie.Areas.Series.Models;
using Microsoft.EntityFrameworkCore;
using Homies.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Homie.Areas.Identity.Models;
using System.Security.Claims;
using Homie.Models;

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

        //public async Task<IActionResult> Index()
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    return View(await db.MoviesEF.Where(a => a.UserUid == userId && a.Archive == false).ToListAsync());
        //}

        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 30;   // количество элементов на странице

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IQueryable<MoviesModel> source = db.MoviesEF.Where(a => a.UserUid == userId && a.Archive == false);
            var count = await source.CountAsync();
            var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            PageViewModel pageViewModel = new PageViewModel(count, page, pageSize);
            IndexViewModel viewModel = new IndexViewModel
            {
                PageViewModel = pageViewModel,
                Series = items
            };
            return View(viewModel);
        }

        //public async Task<IActionResult> ArchMovies()
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    return View(await db.MoviesEF.Where(a => a.UserUid == userId && a.Archive == true).ToListAsync());
        //}

        public async Task<IActionResult> ArchMovies(int page = 1)
        {
            int pageSize = 30;   // количество элементов на странице

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IQueryable<MoviesModel> source = db.MoviesEF.Where(a => a.UserUid == userId && a.Archive == true);
            var count = await source.CountAsync();
            var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            PageViewModel pageViewModel = new PageViewModel(count, page, pageSize);
            IndexViewModel viewModel = new IndexViewModel
            {
                PageViewModel = pageViewModel,
                Series = items
            };
            return View(viewModel);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(MoviesModel movie)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            movie.UserUid = userId;

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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            movie.UserUid = userId;

            db.MoviesEF.Update(movie);
            await db.SaveChangesAsync();

            if (movie.Archive == false)
            {
                return RedirectToAction("Index");
            }
            return RedirectToAction("ArchMovies", "Home", new { area = "Series" });
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