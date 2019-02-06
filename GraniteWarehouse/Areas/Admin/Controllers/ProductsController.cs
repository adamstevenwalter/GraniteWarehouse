using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GraniteWarehouse.Data;
using GraniteWarehouse.Models.ViewModels;
using GraniteWarehouse.Utility;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GraniteWarehouse.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly HostingEnvironment _hostingEnviornment;
        [BindProperty]
        public ProductViewModels ProductsVM { get; set; }
        public ProductsController(ApplicationDbContext db)
        {
            _db = db;
            ProductsVM = new ProductViewModels()
            {
                ProductTypes = _db.ProductTypes.ToList(),
                SpecialTags = _db.SpecialTags.ToList(),
                Products = new Models.Products()
            };
        }

        public async Task<IActionResult> Index()
        {
            var products = _db.Products.Include(m => m.ProductTypes).Include(m => m.SpecialTags);
            return View(await products.ToListAsync());
        }

        //Get: Product Create

        public IActionResult Create()
        {
            return View(ProductsVM); //because I want dropdowns
        }

        //Post: Product Create
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePOST() //we have BindProperty so no parameters are necessary
        {
            if (!ModelState.IsValid)
            {
                return View(ProductsVM);
            }

            _db.Products.Add(ProductsVM.Products);
            await _db.SaveChangesAsync();

            //Product was saved, but not the physical image...

            //Save physical image

            string webRootPath = _hostingEnviornment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var productsFromDb = _db.Products.Find(ProductsVM.Products.Id);

            if (files.Count != 0)
            {
                //Image has been uploaded with form
                var uploads = Path.Combine(webRootPath, SD.ImageFolder);
                var extension = Path.GetExtension(files[0].FileName);

                using (var filestream = new FileStream(Path.Combine(uploads, ProductsVM.Products.Id + extension), FileMode.Create))
                {
                    files[0].CopyTo(filestream); //moves to server and renames
                }

                //now I know the new image name, so I can save the STRING image to the database
                productsFromDb.Image = @"\" + SD.ImageFolder + @"\" + ProductsVM.Products.Id + extension;

            }
        }
    }
}