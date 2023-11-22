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
using System;
using System.IO;

namespace Homie.Areas.Series.Controllers
{
    [Area("Series")]
    [Authorize(Roles = "admin,user")]    
    public class SerieController : Controller
    {
        ApplicationDbContext db;

        //ToDo: заглушки id 55,80 в таблице Picture
        const int notDel55 = 55;
        const int notDel80 = 80;

        public SerieController(ApplicationDbContext context)
        {            
            db = context;
        }
        
        [Breadcrumb(Title = "Список")]
        [HttpGet]        
        public async Task<IActionResult> Index(string name, int page = 1,
            SortState sortOrder = SortState.NameAsc)       
        {
            int pageSize = 40;   // количество элементов на странице

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //фильтрация
            IQueryable<MoviesModel> movies = db.MoviesEF.Where(a => a.UserUid == userId);
            
            if (!String.IsNullOrEmpty(name))
            {
                movies = movies.Where(p => p.Name.Contains(name));
            }

            // сортировка
            switch (sortOrder)
            {
                case SortState.NameDesc:
                    movies = movies.OrderByDescending(s => s.Name);
                    break;
                case SortState.FormatAsc:
                    movies = movies.OrderBy(s => s.Name);
                    break;
                case SortState.FormatDesc:
                    movies = movies.OrderByDescending(s => s.Name);
                    break;
                default:
                    movies = movies.OrderBy(s => s.Name);
                    break;
            }

            // пагинация
            var count = await movies.CountAsync();
            var items = await movies.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            IndexViewModel viewModel = new IndexViewModel
            {
                PageViewModel = new PageViewModel(count, page, pageSize),
                SortViewModel = new SortViewModel(sortOrder),
                MovieFilterViewModel = new MovieFilterViewModel(name),
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
        public async Task<IActionResult> ArchMovies(string name, int page = 1,
            SortState sortOrder = SortState.NameAsc)
        {
            int pageSize = 40;   // количество элементов на странице

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IQueryable<MoviesModel> movies = db.MoviesEF.Where(a => a.UserUid == userId && a.Archive == true);

            if (!String.IsNullOrEmpty(name))
            {
                movies = movies.Where(p => p.Name.Contains(name));
            }

            // сортировка
            switch (sortOrder)
            {
                case SortState.NameDesc:
                    movies = movies.OrderByDescending(s => s.Name);
                    break;
                case SortState.FormatAsc:
                    movies = movies.OrderBy(s => s.Name);
                    break;
                case SortState.FormatDesc:
                    movies = movies.OrderByDescending(s => s.Name);
                    break;
                default:
                    movies = movies.OrderBy(s => s.Name);
                    break;
            }

            // пагинация
            var count = await movies.CountAsync();
            var items = await movies.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            IndexViewModel viewModel = new IndexViewModel
            {
                PageViewModel = new PageViewModel(count, page, pageSize),
                SortViewModel = new SortViewModel(sortOrder),
                MovieFilterViewModel = new MovieFilterViewModel(name),
                Series = items
            };

            return View(viewModel);
        }

        [Breadcrumb(Title = "Добавить")]
        [HttpGet]
        public IActionResult CreateIntoMovies()
        {
            ViewBag.Img_ID_trns = TempData["Image_ID"];            
            ViewBag.Img_UID_trns = TempData["Image_UID"];

            ViewBag.tempNameSerie = TempData["tempName"];
            ViewBag.tempLinkSerie = TempData["tempLink"];
            ViewBag.tempCategorySerie = TempData["tempCategory"];
            ViewBag.tempSeasonSerie = TempData["tempSeason"];
            ViewBag.tempEpisodeSerie = TempData["tempEpisode"];

            //Слово Изображение: Добавлено
            ViewBag.temp_PicAdd = TempData["temp_PicAdd"];

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

            //Картинка заглушка id 55 в таблице Picture
            var plug = await db.Picture.FirstOrDefaultAsync(s => s.Id == notDel80);

            //Разобраться с ImgBT (лишний?)
            if (movie.ImgBT != null)
            {
                var img = await db.Picture.FirstOrDefaultAsync(s => s._uid.ToString() == movie.ImgBT);

                movie.Avatar = img.Avatar;

                //Удаление картинки из Picture, так как картинка помещается в таблицу Picture временно
                Image imgtemp = db.Picture.Where(o => o._uid == Guid.Parse(movie.ImgBT)).FirstOrDefault();

                //Проверка, что не удалится картинка заглушка
                if (imgtemp.Id != notDel80 | imgtemp.Id != notDel55)
                {
                    db.Picture.Remove(imgtemp);
                }
            }
            else
            {
                movie.Avatar = plug.Avatar;
            }

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

        [HttpPost]
        public IActionResult CreateImgSerieIntoMovies(MovieImageModel pvm)
        {
            Image image = new Image { _uid = Guid.NewGuid() };

            if (pvm.AvatarFile != null)
            {
                byte[] imageData = null;
                // считываем переданный файл в массив байтов
                using (var binaryReader = new BinaryReader(pvm.AvatarFile.OpenReadStream()))
                {
                    imageData = binaryReader.ReadBytes((int)pvm.AvatarFile.Length);
                }
                // установка массива байтов
                image.Avatar = imageData;
            }

            db.Picture.Add(image);
            db.SaveChanges();

            TempData["Image_ID"] = image.Id;
            TempData["Image_UID"] = image._uid;
            TempData["temp_PicAdd"] = "Добавлено";

            TempData["tempName"] = pvm.tempName;
            TempData["tempLink"] = pvm.tempLink;
            TempData["tempCategory"] = pvm.tempCategory;
            TempData["tempSeason"] = pvm.tempSeason;
            TempData["tempEpisode"] = pvm.tempEpisode;

            return RedirectToAction("CreateIntoMovies");
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