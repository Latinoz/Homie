using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Homie.Data.Models;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Homie.Models;
using Homie.Areas.Battletech.Models;
using System;
using SmartBreadcrumbs.Attributes;

namespace Homie.Controllers
{
    
    [Authorize(Roles = "admin,user")]    
    public class HomeController : Controller
    {
        ApplicationDbContext db;
        IWebHostEnvironment _appEnvironment;

        public HomeController(ApplicationDbContext context, IWebHostEnvironment appEnvironment)
        {
            db = context;
            _appEnvironment = appEnvironment;
        }

        [DefaultBreadcrumb("Главная")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Fileup()
        {
            return View(db.Files.ToList());
        }

        [HttpPost]
        public async Task<IActionResult> AddFile(IFormFile uploadedFile)
        {
            if (uploadedFile != null)
            {
                // путь к папке Files
                string path = "/Files/" + uploadedFile.FileName;
                // сохраняем файл в папку Files в каталоге wwwroot
                using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }
                FileModel file = new FileModel { Name = uploadedFile.FileName, Path = path };
                db.Files.Add(file);
                db.SaveChanges();
            }

            return RedirectToAction("Fileup");
        }

        [HttpGet]
        public IActionResult FileupImage()
        {
            return View(db.Picture.ToList());
        }

        [HttpPost]
        public IActionResult Create(ImageViewModel pvm)
        {
            Image image = new Image { NameImg = pvm.NameImgVM };
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

            return RedirectToAction("FileupImage");
        }
        
    }
}