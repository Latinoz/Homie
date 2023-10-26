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
using SmartBreadcrumbs.Attributes;

namespace Homie.Areas.Battletech.Controllers
{    
    [Area("Battletech")]
    [Authorize(Roles = "admin,user,battletech")]
    public class HomeController : Controller
    {
        ApplicationDbContext db;

        //заглушка id 55 в таблице Picture
        const int notDel = 55;

        public HomeController(ApplicationDbContext context)
        {
            db = context;
        }
        
        [HttpGet]
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            List<BTMechsModel> mechs = new List<BTMechsModel>();            

            mechs = db.BtEF.Where(a => a.UserUid == userId)
                .Include(u => u.BTPilotsModel).ToList();

            IndexViewModel viewModel = new IndexViewModel
            {
                Mechs = mechs                
            };                      

            return View(viewModel);
        }
        
        public async Task<IActionResult> DelImgIndex(int? Id)
        {
            if (Id != null)
            {
                //Удаление картинки из Picture, если не был Добавлен мех
                Image imgtemp = db.Picture.Where(o => o.Id == Id).FirstOrDefault();

                //Проверка, что не удалится картинка заглушка и картинка существует
                if (imgtemp != null && imgtemp.Id != notDel) 
                {
                   db.Picture.Remove(imgtemp);

                   await db.SaveChangesAsync();
                   return RedirectToAction("Index");                    
                }

                return NotFound();
            }
            return NotFound();
        }
        
        [HttpGet]
        public IActionResult Create()
        {
            BTMechsModel model = new BTMechsModel();

            ViewBag.Img_ID_trns = TempData["Image_ID"];

            //Нужно ViewBag.Img_UID_trns и TempData["Image_UID"]?
            ViewBag.Img_UID_trns = TempData["Image_UID"];

            ViewBag.temp_BV_trns = TempData["temp_BV"];
            ViewBag.temp_Tonnage_trns = TempData["temp_Tonnage"];
            ViewBag.temp_Name_trns = TempData["temp_Name"];            

            //Слово Изображение: Добавлено
            ViewBag.temp_PicAdd = TempData["temp_PicAdd"];

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

            //
            mech.MechUid = Guid.NewGuid().ToString();

            //Картинка заглушка id 55 в таблице Picture
            var plug = await db.Picture.FirstOrDefaultAsync(s => s.Id == notDel);           

            //Разобраться с ImgBT (лишний?)
            if (mech.ImgBT != null)
            {
                var img = await db.Picture.FirstOrDefaultAsync(s => s._uid.ToString() == mech.ImgBT);
                
                mech.Avatar = img.Avatar;

                //Удаление картинки из Picture, так как картинка помещается в таблицу Picture временно
                Image imgtemp = db.Picture.Where(o => o._uid == Guid.Parse(mech.ImgBT)).FirstOrDefault();

                //Проверка, что не удалится картинка заглушка
                if (imgtemp.Id != notDel)
                {
                    db.Picture.Remove(imgtemp);
                }
                
            }
            else
            {
                mech.Avatar = plug.Avatar;               
            }

            db.BtEF.Add(mech);

            await db.SaveChangesAsync();
            return RedirectToAction("Index");
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
            
            TempData["Image_ID"] = image.Id;
            TempData["Image_UID"] = image._uid;
            TempData["temp_PicAdd"] = "Добавлено";

            TempData["temp_BV"] = pvm.tempBV;
            TempData["temp_Tonnage"] = pvm.tempTonnage;
            TempData["temp_Name"] = pvm.tempName;
            TempData["temp_TypeMech"] = pvm.tempTypeMech;            

            return RedirectToAction("Create");
        }
        
        [HttpGet]
        public async Task<IActionResult> Edit(string? id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            BTMechsModel mech = await db.BtEF.FirstOrDefaultAsync(p => p.MechUid == id && p.UserUid == userId);

            if (id != null && mech != null)
            { 
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

            db.BtEF.Update(mech);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> EditImgMech(ImageViewModel pvm)
        {
            //Получение обьекта мех
            BTMechsModel mech = await db.BtEF.FirstOrDefaultAsync(s => s.Id == pvm.tempIdMech);

            if (pvm.AvatarFile != null)
            {
                byte[] imageData = null;
                // считываем переданный файл в массив байтов
                using (var binaryReader = new BinaryReader(pvm.AvatarFile.OpenReadStream()))
                {
                    imageData = binaryReader.ReadBytes((int)pvm.AvatarFile.Length);
                }
                // установка массива байтов                
                mech.Avatar = imageData;
            }

            if(pvm.tempName != null)
            {
                mech.Name = pvm.tempName;
            }
            
            if(pvm.tempTonnage != null)
            {
                mech.Tonnage = (int)pvm.tempTonnage;
            }
            
            if(pvm.tempBV != null)
            {
                mech.Bv = (int)pvm.tempBV;
            }

            if (pvm.tempTypeMech != null)
            {
                mech.TypeMech = pvm.tempTypeMech;
            }
            if (pvm.tempUidMech != null)
            {
                mech.MechUid = pvm.tempUidMech;
            }

            db.BtEF.Update(mech);
            await db.SaveChangesAsync();
            
            return RedirectToAction("Edit", new { id = mech.MechUid});
        }
        
        [HttpGet]
        [ActionName("DeleteImgMech")]
        public async Task<IActionResult> ConfirmDeleteImgMech(string? id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //Получение обьекта мех
            BTMechsModel mech = await db.BtEF.FirstOrDefaultAsync(s => s.MechUid == id && s.UserUid == userId);

            if (id != null && mech != null)
            {
               return View(mech);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteImgMech(string? Id)
        {
            if (Id != null)
            {
                //Получение обьекта мех
                BTMechsModel mech = await db.BtEF.FirstOrDefaultAsync(s => s.MechUid == Id);

                if (mech.Avatar != null)
                {
                    //Картинка заглушка id 55 в таблице Picture
                    var plug = await db.Picture.FirstOrDefaultAsync(s => s.Id == notDel);
                    mech.Avatar = plug.Avatar;                 
                    
                    db.BtEF.Update(mech);
                    await db.SaveChangesAsync();

                    return RedirectToAction("Edit", new { id = mech.MechUid });
                }
            }
            return NotFound();
        }
        
        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(string? id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            BTMechsModel mech = await db.BtEF.FirstOrDefaultAsync(p => p.MechUid == id && p.UserUid == userId);

            if (id != null && mech != null)
            {  
                return View(mech);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string? Id)
        {
            if (Id != null)
            {                
                BTMechsModel mech = await db.BtEF.FirstOrDefaultAsync(p => p.MechUid == Id);

                if (mech != null)
                {
                    db.BtEF.Remove(mech);                    
                    await db.SaveChangesAsync();

                    return RedirectToAction("Index");
                }
            }
            return NotFound();
        } 
    }
}