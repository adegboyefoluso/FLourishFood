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
        private readonly IWebHostEnvironment _webHostEnvironment; //using dependency injection  to get  the webhost environment to access the root file of the (Wwwroot)  getting to
                                                                  //the image path of the  images
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
                // This is for edit
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
                var files = HttpContext.Request.Form.Files;  // retrieving  a new file or image uploaded 
                string webRootPath = _webHostEnvironment.WebRootPath; // Path to wwwroot folder
                if (obj.Product.Id == 0)       
                {
                    //Creating a product
                    string upload = webRootPath + WebConstant.ImagePath;  ///Get the  path to the folder where the image will be save
                    string fileName = Guid.NewGuid().ToString();// generating a guid for the file name 
                    string extension = Path.GetExtension(files[0].FileName);  // Get the file extension

                    using(var filestream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create)) // copy the file to the new location(which is upload)
                    {
                        files[0].CopyTo(filestream);                
                    }

                    //when the file is copied to the new location , The file path in the product object has to be modified.
                    // I am only storing the  fileName  and the extension and not the full file path
                    obj.Product.Image = fileName + extension;
                    _db.Product.Add(obj.Product);
                    _db.SaveChanges();
                }
                else
                {

                    //==> if an image is update, it has to be replace with the new image
                    //==> Retrieve entity from the database and update its property 

                    //Updating      use AsNoTracking to prevent ef tracking tow  product with the same key

                    var objfromDb = _db.Product.AsNoTracking().FirstOrDefault(u => u.Id == obj.Product.Id); //retriving obj from the database
                    if (files.Count > 0)  //if file .count is greater than 0, then ,a new file has been uploaded for an existing product==>generate a file name, extension and copy the file path to the upload folder oin the wwwroot

                    {
                        string upload = webRootPath + WebConstant.ImagePath;  ///GEt the the path to image when we want to save the image
                        string fileName = Guid.NewGuid().ToString(); //Generate a name for the file using a guid
                        string extension = Path.GetExtension(files[0].FileName);  // Get the file extension

                        //Old path 
                        var oldFile = Path.Combine(upload, objfromDb.Image);
                        //Check if the old path exist in the  upload folder,  if it does , Then i will delete if from wwwroot /images/product folder
                        if (System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);
                        }

                        //Copy the new image  to the  wwwroot/images/product folder
                        using (var filestream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                        {
                            files[0].CopyTo(filestream);                  //copy file to new location
                        }
                        obj.Product.Image = fileName + extension; // if image is modified, we change the file name  and extension to the new file name and extension.
                    }

                    //If  file image was not updated , but something else was updated, The imaage will  not change since it was not modified
                    else
                    {
                        obj.Product.Image = objfromDb.Image;

                    }
                    _db.Product.Update(obj.Product);
                    // An issue that came up  is that Entity framework is tracking two  products, The objfromDb  that was retrived from the database and the obj model
                    // This will cause error and antity framework will not know wich product to save because both of them has  the same key/id               ,
                    // to prevent it , Add the linq method of AsNoTracking() when getting object from the database that you dont want EF to track. We are only retriving the objfrmDb just to get the image name and the path 
                    // : refer to line :129

                }
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            
            // if the model state is not valid,  the slect list item are not been populated, the view or the model state that will be returned will not the select list, hence this have to be reloaded again 
            // this will reload all of the drop downs if the model state is not valid.
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

