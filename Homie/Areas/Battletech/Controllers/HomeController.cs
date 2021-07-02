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
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;

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


        [HttpGet]
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    
            IQueryable<BTMechsModel> mechs = db.BTMechsEF.Where(a => a.UserUid == userId);

            var image = db.Picture.ToList();

            IndexViewModel viewModel = new IndexViewModel
            {
                Mechs = mechs,
                Images = image
            };
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new BTMechsModel();
            
            ViewBag.Img_UID_trns = TempData["Image_UID"];

            ViewBag.temp_BV_trns = TempData["temp_BV"];
            ViewBag.temp_Tonnage_trns = TempData["temp_Tonnage"];
            ViewBag.temp_Name_trns = TempData["temp_Name"];

            if (TempData["temp_TypeMech"] == null)
            {                
                ViewBag.temp_TypeMech_trns = model.TypeMechList;                
            }
            else
            {                
                if (TempData.ContainsKey("temp_TypeMech") & TempData["temp_TypeMech"].ToString() != "ASS")
                {
                    //Сюда подставляется переданный тип меха
                    var q = TempData["temp_TypeMech"].ToString();
                    
                    model.TypeMech = q;
                    ViewBag.temp_TypeMech_trns = model.TypeMechList;                    
                }
            }

            return View(model);
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

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id != null)
            {                
                BTMechsModel mech = await db.BTMechsEF.FirstOrDefaultAsync(p => p.Id == id);

                ViewBag.TypeMechsForEdit = mech.TypeMechList;                

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
               

        [HttpPost]
        public IActionResult CreateImgMech(ImageViewModel pvm)
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

            //Здесь сделать удаление картинки, если в Create нажали кнопку "Назад"

            TempData["Image_UID"] = image._uid;

            TempData["temp_BV"] = pvm.tempBV;
            TempData["temp_Tonnage"] = pvm.tempTonnage;
            TempData["temp_Name"] = pvm.tempName;
            TempData["temp_TypeMech"] = pvm.tempTypeMech;

            return RedirectToAction("Create");
        }
    }
}