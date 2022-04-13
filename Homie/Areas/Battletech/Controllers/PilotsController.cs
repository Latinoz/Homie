using Homie.Areas.Battletech.Models;
using Homie.Data.Models;
using Homie.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Homie.Areas.Battletech.Controllers
{
    [Area("Battletech")]
    [Authorize(Roles = "admin,user,battletech")]
    public class PilotsController : Controller
    {
        ApplicationDbContext db;
        

        public PilotsController(ApplicationDbContext context)
        {
            db = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            List<BTPilotsModel> pilots = new List<BTPilotsModel>();

            pilots = db.BtPilotEF.Where(a => a.UserUid == userId)
                .Include(u => u.BTMechsModel).ToList();

            return View(pilots);            
        }

        [HttpGet]
        public IActionResult Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            BTPilotsModel model = new BTPilotsModel();

            List<BTMechsModel> mechs = new List<BTMechsModel>();

            BTMechsModel item = new BTMechsModel();
            item.Id = 0;
            item.Name = "";            

            mechs = db.BtEF.Where(a => a.UserUid == userId && a.BTPilotsModelId == null).ToList();

            mechs.Insert(0, item);

            ViewBag.ListofMechs = mechs;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(BTPilotsModel pilot)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            pilot.UserUid = userId;

            //Добавление GUID для пилота
            pilot.PilotUid = Guid.NewGuid().ToString();

            BTMechsModel mech = db.BtEF.FirstOrDefault(b => b.Id == pilot.MechId);           

            db.BtPilotEF.Add(pilot);
            db.SaveChanges();            

            if(pilot.MechId != 0) 
            {
                mech.BTPilotsModelId = pilot.Id;

                db.BtEF.Update(mech);

                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string? id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            BTPilotsModel pilot = await db.BtPilotEF.FirstOrDefaultAsync(p => p.PilotUid == id && p.UserUid == userId);

            if (id != null && pilot != null)
            {             
               List<BTMechsModel> mechs = db.BtEF.Where(a => a.UserUid == userId).ToList();   
                    
               BTMechsModel item = new BTMechsModel();
               item.Id = 0;
               item.Name = "";                   

               mechs.Insert(0, item);                    

               ViewBag.ListofMechs = mechs;

               return View(pilot);                    
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(BTPilotsModel pilot)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            pilot.UserUid = userId;

            BTMechsModel currentMech = db.BtEF.FirstOrDefault(b => b.BTPilotsModelId == pilot.Id);

            if (pilot.MechId != 0)
            {
                BTMechsModel changedMech = db.BtEF.FirstOrDefault(b => b.Id == pilot.MechId);

                if (currentMech != null)
                {
                    //Убераем связь пилота с текущем мехом
                    currentMech.BTPilotsModelId = null;
                }

                if (changedMech.BTPilotsModelId != null)
                {
                    //Убераем свзяь меха(в котором меняем пилота) и пилота который сейчас связан с ним (MechId)
                    BTPilotsModel changedPilot = db.BtPilotEF.FirstOrDefault(p => p.Id == changedMech.BTPilotsModelId);

                    changedPilot.MechId = 0;
                }

                //Устанавляиваем связь пилота с другим мехом
                changedMech.BTPilotsModelId = pilot.Id;

                db.BtEF.Update(changedMech);

                db.BtPilotEF.Update(pilot);

                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            else 
            {
                if (currentMech != null)
                {
                    //Убераем связь пилота с текущем мехом
                    currentMech.BTPilotsModelId = null;

                    db.BtEF.Update(currentMech);
                }                

                db.BtPilotEF.Update(pilot);

                await db.SaveChangesAsync();

                return RedirectToAction("Index");
                
            }
            
        }

        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(string? id)
        {
            if (id != null)
            {
                BTPilotsModel pilot = await db.BtPilotEF.FirstOrDefaultAsync(p => p.PilotUid == id);
                if (pilot != null)
                    return View(pilot);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string? Id)
        {
            if (Id != null)
            {
                BTPilotsModel pilot = await db.BtPilotEF.FirstOrDefaultAsync(p => p.PilotUid == Id);

                if (pilot != null)
                {
                    //Удалить связь пилота и меха
                    BTMechsModel currentMech = db.BtEF.FirstOrDefault(b => b.BTPilotsModelId == pilot.Id);

                    if (currentMech != null)
                    {
                        //Убераем связь пилота с текущем мехом
                        currentMech.BTPilotsModelId = null;

                        db.BtEF.Update(currentMech);

                        //Удаляем пилота
                        db.BtPilotEF.Remove(pilot);
                        await db.SaveChangesAsync();

                        return RedirectToAction("Index");
                    }

                    db.BtPilotEF.Remove(pilot);
                    await db.SaveChangesAsync();

                    return RedirectToAction("Index");
                }
            }
            return NotFound();
        }
    }
}
