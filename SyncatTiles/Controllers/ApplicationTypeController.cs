using Microsoft.AspNetCore.Mvc;
using SyncatTiles.Data;
using SyncatTiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SyncatTiles.Controllers
{
    public class ApplicationTypeController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ApplicationTypeController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            IEnumerable<ApplicationType> objList = _db.ApplicationType;
            return View(objList);
        }
        //Get:Create
        public IActionResult Create()
        {
            return View();
        }
        //Get:Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ApplicationType model)
        {
            if (ModelState.IsValid)
            {
                _db.ApplicationType.Add(model);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(model);
            
        }

        //Get -Edit
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _db.ApplicationType.Find(id);
            if(obj is null)
            {
                return NotFound();
            }
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //Post- Edit
        public IActionResult Edit (ApplicationType obj)
        {

            if (ModelState.IsValid)
            {
                _db.ApplicationType.Update(obj);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                return View(obj);
            }
            
        }

        //Get- Delete

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _db.ApplicationType.Find(id);
            if (obj is null)
            {
                return NotFound();
            }
            return View(obj);
        }
        //Post -Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int id)
        {
            var obj = _db.ApplicationType.Find(id);
            if (obj is null)
            {
                return NotFound();
            }
                _db.ApplicationType.Remove(obj);
                _db.SaveChanges();
                return RedirectToAction("Index");
                       
        }

    }
}
