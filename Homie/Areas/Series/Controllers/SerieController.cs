using System.Collections.Generic;
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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using System.Security.Cryptography;

namespace Homie.Areas.Series.Controllers
{
    [Area("Series")]
    [Authorize(Roles = "admin,user")]
    public class SerieController : Controller
    {
    ApplicationDbContext db;
    private readonly FileUploadSettings _fileUploadSettings;
    private readonly IMemoryCache _cache;

        //ToDo: заглушки id 55,80 в таблице Picture
        const int notDel55 = 55;
        const int notDel80 = 80;

        /// <summary>
        /// Валидация загружаемого файла изображения
        /// </summary>
        /// <param name="file">Загружаемый файл</param>
        /// <returns>Результат валидации с сообщением об ошибке</returns>
        private (bool IsValid, string ErrorMessage) ValidateImageFile(IFormFile file)
        {
            if (file == null)
                return (true, null);

            // Проверка размера файла
            if (file.Length > _fileUploadSettings.MaxFileSizeBytes)
            {
                return (false, $"Размер файла превышает максимально допустимый размер {_fileUploadSettings.MaxFileSizeBytes / (1024 * 1024)} MB");
            }

            // Проверка типа файла
            if (!_fileUploadSettings.AllowedImageTypes.Contains(file.ContentType.ToLower()))
            {
                return (false, "Неподдерживаемый тип файла. Разрешены только: JPEG, JPG, PNG, GIF, BMP");
            }

            // Проверка расширения файла
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            if (!allowedExtensions.Contains(extension))
            {
                return (false, "Неподдерживаемое расширение файла. Разрешены только: .jpg, .jpeg, .png, .gif, .bmp");
            }

            return (true, null);
        }

        public SerieController(ApplicationDbContext context, FileUploadSettings fileUploadSettings, IMemoryCache cache)
        {
            db = context;
            _fileUploadSettings = fileUploadSettings;
            _cache = cache;
        }

    /// <summary>
    /// Возвращает байты изображения (аватар) для фильма в виде результата File.
    /// Это позволяет браузеру запрашивать изображения отдельно (параллельно, с возможностью кэширования и отложенной загрузки),
    /// вместо встраивания больших base64-строк непосредственно в HTML страницы.
    /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Avatar(int id, int? width = null, int? height = null)
        {
            // Выбираем только столбец Avatar для указанного фильма
            var avatar = await db.MoviesEF
                .Where(m => m.Id == id)
                .Select(m => m.Avatar)
                .FirstOrDefaultAsync();

            if (avatar == null || avatar.Length == 0)
            {
                return NotFound();
            }

            byte[] resultBytes;

            // Определим формат исходного изображения (если возможно) — нужен для корректного Content-Type
            var detectedFormat = SixLabors.ImageSharp.Image.DetectFormat(avatar);
            var detectedMime = detectedFormat?.DefaultMimeType ?? "image/jpeg";

            if (width.HasValue || height.HasValue)
            {
                // Генерируем thumbnail и кэшируем его в IMemoryCache
                var w = width ?? 0;
                var h = height ?? 0;
                var cacheKey = $"avatar:{id}:{w}x{h}";

                if (!_cache.TryGetValue(cacheKey, out byte[] cachedBytes))
                {
                    // Загружаем изображение и ресайзим
                    using var imgSharp = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(avatar);

                    var resizeOptions = new SixLabors.ImageSharp.Processing.ResizeOptions
                    {
                        Mode = SixLabors.ImageSharp.Processing.ResizeMode.Max,
                        Size = new SixLabors.ImageSharp.Size(w > 0 ? w : imgSharp.Width, h > 0 ? h : imgSharp.Height)
                    };

                    imgSharp.Mutate(x => x.Resize(resizeOptions));

                    using var ms = new MemoryStream();

                    // Если исходный формат поддерживает альфу (PNG, GIF, WebP), сохраняем как PNG чтобы не терять прозрачность,
                    // иначе используем JPEG для меньшего размера.
                    var fmtName = detectedFormat?.Name?.ToLowerInvariant() ?? string.Empty;
                    if (fmtName.Contains("png") || fmtName.Contains("gif") || fmtName.Contains("webp"))
                    {
                        var encoderPng = new SixLabors.ImageSharp.Formats.Png.PngEncoder();
                        imgSharp.Save(ms, encoderPng);
                        detectedMime = "image/png";
                    }
                    else
                    {
                        var encoder = new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder { Quality = 80 };
                        imgSharp.Save(ms, encoder);
                        detectedMime = "image/jpeg";
                    }

                    cachedBytes = ms.ToArray();

                    // Кешируем на некоторое время
                    _cache.Set(cacheKey, cachedBytes, new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromHours(6)
                    });
                }

                resultBytes = cachedBytes;
            }
            else
            {
                // Возвращаем оригинал
                resultBytes = avatar;
            }

            // ETag по SHA-256 содержимого
            var etag = Convert.ToBase64String(SHA256.HashData(resultBytes));
            var requestEtag = Request.Headers["If-None-Match"].FirstOrDefault();
            if (!string.IsNullOrEmpty(requestEtag) && requestEtag == etag)
            {
                return StatusCode(304);
            }

            Response.Headers["ETag"] = etag;
            Response.Headers["Cache-Control"] = "public, max-age=86400"; // кэшировать 1 день

            return File(resultBytes, detectedMime);
        }

        [Breadcrumb(Title = "Список")]
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1,
            SortState sortOrder = SortState.NameAsc)
        {
            int pageSize = 10;   // количество элементов на странице

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IQueryable<MoviesModel> movies = db.MoviesEF.Where(a => a.UserUid == userId && a.Favorite == false);

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
            var items = await movies
                .Select(m => new Homie.Areas.Series.Models.MovieListDto {
                    Id = m.Id,
                    Name = m.Name,
                    Link = m.Link,
                    Category = m.Category,
                    Season = m.Season,
                    Episode = m.Episode,
                    HoldPlay = m.HoldPlay,
                    Favorite = m.Favorite,
                    UserUid = m.UserUid,
                    ImgBT = m.ImgBT
                })
                .Skip((page - 1) * pageSize).Take(pageSize).AsNoTracking().ToListAsync();

            IndexViewModel viewModel = new IndexViewModel
            {
                PageViewModel = new PageViewModel(count, page, pageSize),
                SortViewModel = new SortViewModel(sortOrder),
                Series = items
            };

            return View(viewModel);
        }

        [Breadcrumb(Title = "Избранное")]
        [HttpGet]
        public async Task<IActionResult> Favorite(int page = 1,
            SortState sortOrder = SortState.NameAsc)
        {
            int pageSize = 10;   // количество элементов на странице

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IQueryable<MoviesModel> movies = db.MoviesEF.Where(a => a.UserUid == userId && a.Favorite == true);

            var count = await movies.CountAsync();
            var items = await movies
                .Select(m => new Homie.Areas.Series.Models.MovieListDto {
                    Id = m.Id,
                    Name = m.Name,
                    Link = m.Link,
                    Category = m.Category,
                    Season = m.Season,
                    Episode = m.Episode,
                    HoldPlay = m.HoldPlay,
                    Favorite = m.Favorite,
                    UserUid = m.UserUid,
                    ImgBT = m.ImgBT
                })
                .Skip((page - 1) * pageSize).Take(pageSize).AsNoTracking().ToListAsync();

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

        [Breadcrumb(Title = "Добавить")]
        [HttpGet]
        public IActionResult CreateIntoFavorite()
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

        [HttpPost]
        public async Task<IActionResult> CreateIntoMovies(MoviesModel movie)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            movie.UserUid = userId;

            //Картинка заглушка id 80 в таблице Picture
            var plug = await db.Picture.FirstOrDefaultAsync(s => s.Id == notDel80);

            //Разобраться с ImgBT (лишний?)
            if (movie.ImgBT != null)
            {
                var img = await db.Picture.FirstOrDefaultAsync(s => s._uid.ToString() == movie.ImgBT);

                movie.Avatar = img.Avatar;

                //Удаление картинки из Picture, так как картинка помещается в таблицу Picture временно
                Homie.Models.Image imgtemp = db.Picture.Where(o => o._uid == Guid.Parse(movie.ImgBT)).FirstOrDefault();

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
        public async Task<IActionResult> CreateIntoFavorite(MoviesModel movie)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            movie.UserUid = userId;
            movie.Favorite = true;

            //Картинка заглушка id 80 в таблице Picture
            var plug = await db.Picture.FirstOrDefaultAsync(s => s.Id == notDel80);

            //Разобраться с ImgBT (лишний?)
            if (movie.ImgBT != null)
            {
                var img = await db.Picture.FirstOrDefaultAsync(s => s._uid.ToString() == movie.ImgBT);

                movie.Avatar = img.Avatar;

                //Удаление картинки из Picture, так как картинка помещается в таблицу Picture временно
                Homie.Models.Image imgtemp = db.Picture.Where(o => o._uid == Guid.Parse(movie.ImgBT)).FirstOrDefault();

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
            return RedirectToAction("Favorite");
        }

        [HttpPost]
        public IActionResult CreateImgSerieIntoMovies(MovieImageModel pvm)
        {
            Homie.Models.Image image = new Homie.Models.Image { _uid = Guid.NewGuid() };

            if (pvm.AvatarFile != null)
            {
                // Валидация загружаемого файла
                var validation = ValidateImageFile(pvm.AvatarFile);
                if (!validation.IsValid)
                {
                    TempData["ErrorMessage"] = validation.ErrorMessage;
                    
                    // Сохраняем временные данные для возврата на форму
                    TempData["tempName"] = pvm.tempName;
                    TempData["tempLink"] = pvm.tempLink;
                    TempData["tempCategory"] = pvm.tempCategory;
                    TempData["tempSeason"] = pvm.tempSeason;
                    TempData["tempEpisode"] = pvm.tempEpisode;
                    
                    return RedirectToAction("CreateIntoMovies");
                }

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


        public async Task<IActionResult> GoToFavorite(int? Id)
        {
            if (Id != null)
            {
                MoviesModel movie = await db.MoviesEF.FirstOrDefaultAsync(p => p.Id == Id);
                movie.Favorite = true;

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return NotFound();
        }

        public async Task<IActionResult> GoToIndex(int? Id)
        {
            if (Id != null)
            {
                MoviesModel movie = await db.MoviesEF.FirstOrDefaultAsync(p => p.Id == Id);
                movie.Favorite = false;

                await db.SaveChangesAsync();
                return RedirectToAction("Favorite");
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

            // Проверка флага удаления изображения, пришедшего из формы
            var deleteFlag = Request.Form["DeleteImage"].FirstOrDefault();
            var isDelete = !string.IsNullOrEmpty(deleteFlag) && deleteFlag.ToLower() == "true";
            if (isDelete)
            {
                // Картинка-заглушка id 80 в таблице Picture
                var plug = await db.Picture.FirstOrDefaultAsync(s => s.Id == notDel80);
                if (plug != null)
                {
                    movie.Avatar = plug.Avatar;
                }
                else
                {
                    movie.Avatar = null;
                }
            }

            db.MoviesEF.Update(movie);
            await db.SaveChangesAsync();

            // Если была операция удаления изображения — очистим кэш для миниатюр этой записи
            if (isDelete)
            {
                try
                {
                    // Удаляем кэшированные варианты изображений, которые используются на страницах
                    // (в Index/Favorite используются width=200,height=260)
                    _cache.Remove($"avatar:{movie.Id}:200x260");
                    // дополнительные размеры, которые могут использоваться в других местах
                    _cache.Remove($"avatar:{movie.Id}:180x240");
                    _cache.Remove($"avatar:{movie.Id}:100x130");
                }
                catch
                {
                    // игнорируем ошибки кэша
                }

                // Перенаправляем обратно в Edit чтобы показать обновлённую заглушку
                return RedirectToAction("Edit", new { id = movie.Id });
            }

            if (movie.Favorite == true)
            {
                return RedirectToAction("Favorite", "Serie", new { area = "Series" });
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> EditImgMovie(MovieImageModel pvm)
        {
            MoviesModel movies = await db.MoviesEF.FirstOrDefaultAsync(s => s.Id == pvm.tempIdMovie);

            if (pvm.AvatarFile != null)
            {
                // Валидация загружаемого файла
                var validation = ValidateImageFile(pvm.AvatarFile);
                if (!validation.IsValid)
                {
                    TempData["ErrorMessage"] = validation.ErrorMessage;
                    return RedirectToAction("Edit", new { id = movies.Id });
                }

                byte[] imageData = null;
                // считываем переданный файл в массив байтов
                using (var binaryReader = new BinaryReader(pvm.AvatarFile.OpenReadStream()))
                {
                    imageData = binaryReader.ReadBytes((int)pvm.AvatarFile.Length);
                }
                // установка массива байтов                
                movies.Avatar = imageData;
            }

            if (pvm.tempName != null)
            {
                movies.Name = pvm.tempName;
            }

            if (pvm.tempLink != null)
            {
                movies.Link = pvm.tempLink;
            }

            if (pvm.tempCategory != null)
            {
                movies.Category = pvm.tempCategory;
            }

            if (pvm.tempSeason != null)
            {
                movies.Season = (int)pvm.tempSeason;
            }
            if (pvm.tempEpisode != null)
            {
                movies.Episode = (int)pvm.tempEpisode;
            }

            if (pvm.tempHoldPlay != null)
            {
                movies.HoldPlay = pvm.tempHoldPlay;
            }

            db.MoviesEF.Update(movies);
            await db.SaveChangesAsync();

            return RedirectToAction("Edit", new { id = movies.Id });
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

                    if (movie.Favorite == false)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return RedirectToAction("Favorite");
                    }
                }
            }
            return NotFound();
        }

        [Breadcrumb(Title = "Удалить")]
        [HttpGet]
        [ActionName("DeleteImgMovie")]
        public async Task<IActionResult> ConfirmDeleteImgMovie(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            MoviesModel movie = await db.MoviesEF.FirstOrDefaultAsync(s => s.Id == id && s.UserUid == userId);

            if (movie != null)
            {
                return View(movie);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteImgMovie(int Id)
        {
            MoviesModel movie = await db.MoviesEF.FirstOrDefaultAsync(s => s.Id == Id);

            if (movie == null)
            {
                return NotFound();
            }

            if (movie.Avatar != null)
            {
                //Картинка заглушка id 80 в таблице Picture
                var plug = await db.Picture.FirstOrDefaultAsync(s => s.Id == notDel80);
                movie.Avatar = plug.Avatar;

                db.MoviesEF.Update(movie);
                await db.SaveChangesAsync();

                return RedirectToAction("Edit", new { id = movie.Id });
            }

            return NotFound();
        }

        /// <summary>
        /// API endpoint для автодополнения поиска сериалов
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchMovies(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Json(new List<object>());
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Поиск по названию сериала
            var movies = await db.MoviesEF
                .Where(m => m.UserUid == userId && m.Name.Contains(term))
                .OrderBy(m => m.Name)
                .Take(20) // Ограничиваем количество результатов
                .Select(m => new
                {
                    id = m.Id,
                    label = m.Name,
                    value = m.Name
                })
                .ToListAsync();

            return Json(movies);
        }
    }
}