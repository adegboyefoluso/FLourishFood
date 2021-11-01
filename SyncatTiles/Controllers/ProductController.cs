using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SyncatTiles.Data;
using SyncatTiles.Models;
using SyncatTiles.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SyncatTiles.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment; //using dependency injection  to get  the webhost environment
        public ProductController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {//                                           ==>using Eager Loading to load all category and applicationType rather than goint back to the database to load them
            IEnumerable<Product> objList = _db.Product.Include(u=>u.Category).Include(u=>u.ApplicationType);

            //foreach(var obj in objList)
            //{
            //    obj.Category = _db.Category.SingleOrDefault(e => e.Id == obj.CategoryId);
            //    obj.ApplicationType = _db.ApplicationType.SingleOrDefault(e => e.Id == obj.ApplicationTypeId);
            //}
            return View(objList);
        }


        //Get - Upsert   To create and Edit Product in a single method
        // if the id is null then the create methosd will be callled and if the id is not null then the edit method is called

        public IActionResult Upsert(int? id)
        {

            //=======Using View Model=======//
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategorySelectgList = _db.Category.Select(e => new SelectListItem
                {
                    Text = e.CategoryName,
                    Value = e.Id.ToString()
                }),
                ApplicationTypeList= _db.ApplicationType.Select(e => new SelectListItem
                {
                    Text = e.Name,
                    Value = e.Id.ToString()
                }),

            };


            if (id == null)
            {
                // this is for create
                return View(productVM);
            }
            else
            {
                productVM.Product = _db.Product.Find(id);
                if (productVM.Product is null)
                {
                    return NotFound();
                }
                return View(productVM);
            }
            //====================Using View bag or View Data===========================//
            //IEnumerable<SelectListItem> CategoryDropDown = _db.Category.Select(e => new SelectListItem  // using view bag  to transfer data from controller to view
            //{
            //    Text = e.CategoryName,
            //    Value = e.Id.ToString()
            //});

            ////ViewBag.CategoryDropDown = CategoryDropDown;      //Using ViewBag
            //ViewData["CategoryDropDown"] = CategoryDropDown;    //using ViewData instaed of view bag- It uses Dictionary  for key and the value pair


            //Product product = new Product();


        }

        ////Post-Upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM obj)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnvironment.WebRootPath;
                if (obj.Product.Id == 0)
                {
                    //Creating a product
                    string upload = webRootPath + WebConstant.ImagePath;  ///GEt the the path to image when we want to save the image
                    string fileName = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(files[0].FileName);  // Get the file extension

                    using(var filestream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(filestream);                  //copy file to new location
                    }

                    obj.Product.Image = fileName + extension;
                    _db.Product.Add(obj.Product);
                    _db.SaveChanges();
                }
                else
                {
                    //Updating                   use AsNoTracking to prevent ef tracking tow  product with the same key
                    var objfromDb = _db.Product.AsNoTracking().FirstOrDefault(u => u.Id == obj.Product.Id); 
                    if (files.Count > 0)  // if file is updated
                    {
                        string upload = webRootPath + WebConstant.ImagePath;  ///GEt the the path to image when we want to save the image
                        string fileName = Guid.NewGuid().ToString();
                        string extension = Path.GetExtension(files[0].FileName);  // Get the file extension

                        var oldFile = Path.Combine(upload, objfromDb.Image);
                        if (System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);
                        }

                        using (var filestream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                        {
                            files[0].CopyTo(filestream);                  //copy file to new location
                        }
                        obj.Product.Image = fileName + extension; // if image is modified, we change the file name  and extension
                    }

                    else
                    {
                        obj.Product.Image = objfromDb.Image;

                    }
                    _db.Product.Update(obj.Product);
                }
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            obj.CategorySelectgList = _db.Category.Select(e => new SelectListItem
            {
                Text = e.CategoryName,
                Value = e.Id.ToString()
            });
            obj.ApplicationTypeList = _db.ApplicationType.Select(e => new SelectListItem
            {
                Text = e.Name,
                Value = e.Id.ToString()
            });
            return View(obj);
        }



        // Get-Delete
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            //using Eager loading->This is a way of telling  EF that when you load a  product also perfom a  joint operation and load the corresponding  category
            var obj = _db.Product.Include(u => u.Category).Include(u=>u.ApplicationType).SingleOrDefault(u => u.Id == id);  // The find mnethod works only on primary key
           
           
            if (obj == null)
            {
                return NotFound();
            }

            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]


        public IActionResult DeletePost(int? id)
        {

            var obj = _db.Product.Find(id);  // The find mnethod works only on primary key
            if (obj == null)
            {
                return NotFound();
            }
            string webRootPath = _webHostEnvironment.WebRootPath;
            string upload = webRootPath + WebConstant.ImagePath;  ///GEt the the path to image when we want to save the image
           
            var oldFile = Path.Combine(upload, obj.Image);
            if (System.IO.File.Exists(oldFile))

            {
                System.IO.File.Delete(oldFile);
            }
            _db.Product.Remove(obj);
            _db.SaveChanges();
            return RedirectToAction("Index");


        }
    }
}

