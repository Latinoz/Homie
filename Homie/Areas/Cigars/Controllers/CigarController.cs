﻿using System;
using System.Collections.Generic;
using Homie.Areas.Cigars.Models;
using System.Linq;
using System.Threading.Tasks;
using Homie.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Homie.Models;
using SmartBreadcrumbs.Attributes;

namespace Homie.Areas.Cigars.Controllers
{
    [Area("Cigars")]
    [Authorize(Roles = "admin,user")]
    public class CigarController : Controller
    {
        ApplicationDbContext db;

        public CigarController(ApplicationDbContext context)
        {
            db = context;
        }
        
        public async Task<IActionResult> Index(int? format, string name, int page = 1,
            SortState sortOrder = SortState.NameAsc)
        {
            int pageSize = 30;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //фильтрация
            IQueryable<CigarsModel> cigars = db.CigarsEF.Where(a => a.UserUid == userId);

            if (format != null && format != 0)
            {
                cigars = cigars.Where(p => p.FormatId == format);
            }
            if (!String.IsNullOrEmpty(name))
            {
                cigars = cigars.Where(p => p.Name.Contains(name));
            }

            // сортировка
            switch (sortOrder)
            {
                case SortState.NameDesc:
                    cigars = cigars.OrderByDescending(s => s.Name);
                    break;                
                case SortState.FormatAsc:
                    cigars = cigars.OrderBy(s => s.Format.ShapeName);
                    break;
                case SortState.FormatDesc:
                    cigars = cigars.OrderByDescending(s => s.Format.ShapeName);
                    break;
                default:
                    cigars = cigars.OrderBy(s => s.Name);
                    break;
            }

            // пагинация
            var count = await cigars.CountAsync();
            var items = await cigars.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            
            IndexViewModel viewModel = new IndexViewModel
            {
                PageViewModel = new PageViewModel(count, page, pageSize),
                SortViewModel = new SortViewModel(sortOrder),
                CigarFilterViewModel = new CigarFilterViewModel(db.FormatsEF.ToList(), format, name),
                Cigars = items
            };
            return View(viewModel);
        }
        
        public IActionResult Create()
        {
            List<Format> shapeslist = new List<Format>();

            shapeslist = db.FormatsEF.ToList();

            ViewBag.ListofShapes = shapeslist;           

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CigarsModel cigar)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            cigar.UserUid = userId;

            Format format = new Format();            

            int idFromShape = cigar.FormatId;

            format = db.FormatsEF.FirstOrDefault(p => p.Id == idFromShape);

            cigar.Shape = format.ShapeName;

            db.CigarsEF.Add(cigar);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        
        public async Task<IActionResult> Edit(int? id)
        {
            List<Format> shapeslist = new List<Format>();

            shapeslist = db.FormatsEF.ToList();

            ViewBag.ListofShapes = shapeslist;

            if (id != null)
            {
                CigarsModel cigar = await db.CigarsEF.FirstOrDefaultAsync(p => p.Id == id);
                if (cigar != null)
                    return View(cigar);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CigarsModel cigar)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            cigar.UserUid = userId;

            Format format = new Format();

            int idFromShape = cigar.FormatId;

            format = db.FormatsEF.FirstOrDefault(p => p.Id == idFromShape);

            cigar.Shape = format.ShapeName;

            db.CigarsEF.Update(cigar);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        
        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int? id)
        {
            if (id != null)
            {
                CigarsModel cigar = await db.CigarsEF.FirstOrDefaultAsync(p => p.Id == id);
                if (cigar != null)
                    return View(cigar);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id != null)
            {
                CigarsModel cigar = await db.CigarsEF.FirstOrDefaultAsync(p => p.Id == Id);
                if (cigar != null)
                {
                    db.CigarsEF.Remove(cigar);
                    await db.SaveChangesAsync();

                    return RedirectToAction("Index");
                    
                }
            }
            return NotFound();
        }
    }
}