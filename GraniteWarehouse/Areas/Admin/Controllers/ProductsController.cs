using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GraniteWarehouse.Data;
using GraniteWarehouse.Models;
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
        public ProductsController(ApplicationDbContext db, HostingEnvironment hostingEnvironment)
        {
            _db = db;
            _hostingEnviornment = hostingEnvironment;
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

            //saves the information from the form to the database, except for the image
            _db.Products.Add(ProductsVM.Products);
            await _db.SaveChangesAsync();

            //Product was saved, but not the physical image..
            //Save physical image
            //path to wwwroot since it varies based on hosting enviornment
            string webRootPath = _hostingEnviornment.WebRootPath;
            //any files attatched to the form get added to this array
            var files = HttpContext.Request.Form.Files;
            //find the Id of the product we are adding
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
            else
            {
                //user didn't give us an image so we'll upload the placeholder image
                var uploads = Path.Combine(webRootPath, SD.ImageFolder + @"\" + SD.DefaultProductImage);
                System.IO.File.Copy(uploads, webRootPath + @"\" + SD.ImageFolder + @"\" + ProductsVM.Products.Id + ".jpg");
                productsFromDb.Image = @"\" + SD.ImageFolder + @"\" + ProductsVM.Products.Id + ".jpg";
            }

            //save again so that the image path actually gets saved to the database
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //Get Edit
        public async Task<IActionResult> Edit (int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ProductsVM.Products = await _db.Products
                .Include(m => m.SpecialTags)
                .Include(m => m.ProductTypes)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (ProductsVM.Products == null)
            {
                return NotFound();
            }

            return View(ProductsVM);
        }

        //Post : Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            if (ModelState.IsValid)
            {
                string webRootPath = _hostingEnviornment.WebRootPath;
                var files = HttpContext.Request.Form.Files;

                var productFromDb = _db.Products.Where(m => m.Id == ProductsVM.Products.Id).FirstOrDefault();

                if (files.Count > 0 && files[0] != null)
                {
                    //if user uploads a new image
                    var uploads = Path.Combine(webRootPath, SD.ImageFolder);
                    var extension_new = Path.GetExtension(files[0].FileName);
                    var extension_old = Path.GetExtension(productFromDb.Image);

                    if (System.IO.File.Exists(Path.Combine(uploads, ProductsVM.Products.Id + extension_old)))
                    {
                        System.IO.File.Delete(Path.Combine(uploads, ProductsVM.Products.Id + extension_old));
                    }
                    using (var filestream = new FileStream(Path.Combine(uploads, ProductsVM.Products.Id + extension_new), FileMode.Create))
                    {
                        files[0].CopyTo(filestream);
                    }
                    ProductsVM.Products.Image = @"\" + SD.ImageFolder + @"\" + ProductsVM.Products.Id + extension_new;
                }

                if (ProductsVM.Products.Image != null)
                {
                    productFromDb.Image = ProductsVM.Products.Image;
                }

                productFromDb.Name = ProductsVM.Products.Name;
                productFromDb.Price = ProductsVM.Products.Price;
                productFromDb.Available = ProductsVM.Products.Available;
                productFromDb.ProductTypeId = ProductsVM.Products.ProductTypeId;
                productFromDb.SpecialTagsId = ProductsVM.Products.SpecialTagsId;
                productFromDb.ShadeColor = ProductsVM.Products.ShadeColor;
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(ProductsVM);
        }

        //Get Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ProductsVM.Products = await _db.Products
                .Include(m => m.SpecialTags)
                .Include(m => m.ProductTypes)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (ProductsVM.Products == null)
            {
                return NotFound();
            }

            return View(ProductsVM);
        }

        //Get Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ProductsVM.Products = await _db.Products
                .Include(m => m.SpecialTags)
                .Include(m => m.ProductTypes)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (ProductsVM.Products == null)
            {
                return NotFound();
            }

            return View(ProductsVM);
        }

        //POST : Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            string webRootPath = _hostingEnviornment.WebRootPath;
            Products products = await _db.Products.FindAsync(id);

            if (products == null)
            {
                return NotFound();
            }
            else
            {
                var uploads = Path.Combine(webRootPath, SD.ImageFolder);
                var extension = Path.GetExtension(products.Image);

                if (System.IO.File.Exists(Path.Combine(uploads, products.Id + extension)))
                {
                    System.IO.File.Delete(Path.Combine(uploads, products.Id + extension));
                }
                _db.Products.Remove(products);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
        }
    }
}