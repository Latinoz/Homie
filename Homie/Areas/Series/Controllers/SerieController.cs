using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Homie.Areas.Series.Models;
using Microsoft.EntityFrameworkCore;
using Homie.Data.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Homie.Models;
using SmartBreadcrumbs.Attributes;

namespace Homie.Areas.Series.Controllers
{
    [Area("Series")]
    [Authorize(Roles = "admin,user")]    
    public class SerieController : Controller
    {
        ApplicationDbContext db;

        public SerieController(ApplicationDbContext context)
        {            
            db = context;
        }

        [DefaultBreadcrumb("Главная")]        
        [HttpGet]        
        public async Task<IActionResult> Index(int page = 1)       {
            int pageSize = 40;   // количество элементов на странице

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IQueryable<MoviesModel> source = db.MoviesEF.Where(a => a.UserUid == userId && a.Archive == false && a.Watching == false);
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

        [Breadcrumb(Title = "Просмотр")]
        [HttpGet]
        public async Task<IActionResult> Watching(int page = 1)
        {
            int pageSize = 20;   // количество элементов на странице

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IQueryable<MoviesModel> source = db.MoviesEF.Where(a => a.UserUid == userId && a.Archive == false && a.Watching == true);
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

        [Breadcrumb(Title = "Архив")]
        [HttpGet]
        public async Task<IActionResult> ArchMovies(int page = 1)
        {
            int pageSize = 50;   // количество элементов на странице

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

        [Breadcrumb(Title = "Добавить")]
        [HttpGet]
        public IActionResult CreateIntoMovies()
        {
            return View();
        }
        
        [HttpGet]
        public IActionResult CreateIntoWatching()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateIntoMovies(MoviesModel movie)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            movie.UserUid = userId;

            db.MoviesEF.Add(movie);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateIntoWatching(MoviesModel movie)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            movie.UserUid = userId;

            movie.Watching = true;

            db.MoviesEF.Add(movie);
            await db.SaveChangesAsync();
            return RedirectToAction("Watching");
        }
        
        public async Task<IActionResult> GoToArchive(int? Id)
        {
            if (Id != null)
            {
                MoviesModel movie = await db.MoviesEF.FirstOrDefaultAsync(p => p.Id == Id);
                movie.Archive = true;                

                await db.SaveChangesAsync();
                
                if (movie.Watching == true)
                {
                    return RedirectToAction("Watching", "Serie", new { area = "Series" });
                }

                return RedirectToAction("Index");
            }            
            return NotFound();
        }

        public async Task<IActionResult> FromArchiveGoToIndex(int? Id)
        {
            if (Id != null)
            {
                MoviesModel movie = await db.MoviesEF.FirstOrDefaultAsync(p => p.Id == Id);
                movie.Archive = false;
                movie.Watching = false;

                await db.SaveChangesAsync();
                return RedirectToAction("ArchMovies");
            }
            return NotFound();
        }

        public async Task<IActionResult> GoToWatching(int? Id)
        {
            if (Id != null)
            {
                MoviesModel movie = await db.MoviesEF.FirstOrDefaultAsync(p => p.Id == Id);
                movie.Watching = true;

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return NotFound();
        }

        public async Task<IActionResult> FromArchiveGoToWatching(int? Id)
        {
            if (Id != null)
            {
                MoviesModel movie = await db.MoviesEF.FirstOrDefaultAsync(p => p.Id == Id);
                movie.Archive = false;
                movie.Watching = true;

                await db.SaveChangesAsync();
                return RedirectToAction("ArchMovies");
            }
            return NotFound();
        }

        public async Task<IActionResult> FromWatchingGoToIndex(int? Id)
        {
            if (Id != null)
            {
                MoviesModel movie = await db.MoviesEF.FirstOrDefaultAsync(p => p.Id == Id);
                movie.Watching = false;

                await db.SaveChangesAsync();
                return RedirectToAction("Watching");
            }
            return NotFound();
        }

        [Breadcrumb(Title = "Изменить")]
        [HttpGet]
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

            if (movie.Archive == true && movie.Watching == false)
            {
                return RedirectToAction("ArchMovies", "Serie", new { area = "Series" });
            }
            else if(movie.Archive == false && movie.Watching == true)
            {
                return RedirectToAction("Watching", "Serie", new { area = "Series" });
            }            
            return RedirectToAction("Index");
        }

        [Breadcrumb(Title = "Удалить")]
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

                    if(movie.Archive == false && movie.Watching == false)
                    {
                        return RedirectToAction("Index");
                    }
                    else if(movie.Archive == false && movie.Watching == true)
                    {
                        return RedirectToAction("Watching");
                    }
                    else
                    {
                        return RedirectToAction("ArchMovies", "Serie", new { area = "Series" });
                    }                    
                }
            }
            return NotFound();
        }
    }
}