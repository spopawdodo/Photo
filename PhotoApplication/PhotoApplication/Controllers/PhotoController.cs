using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Http;
using PhotoApplication.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace PhotoApplication.Controllers
{
    public class PhotoController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private int _perPage = 12;

        public ActionResult Index()
        {
            var photos = db.Photos.Include("Category").Include("Album").OrderByDescending(a => a.Date);
            var totalItems = photos.Count();
            var currentPage = Convert.ToInt32(Request.Params.Get("page"));

            var offset = 0;

            if (!currentPage.Equals(0)) { 
                offset = (currentPage - 1) * this._perPage;
            }

            var paginatedPhotos = photos.Skip(offset).Take(this._perPage);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            ViewBag.perPage = this._perPage;
            ViewBag.total = totalItems;
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)this._perPage);
            ViewBag.Photos = paginatedPhotos;

            return View();
        }


        public ActionResult Show(int id)
        {
            Photo photo = db.Photos.Find(id);

            ViewBag.displayButtons = false;
            if (photo.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {
                ViewBag.displayButtons = true;
            }
            
            ViewBag.isAdmin = User.IsInRole("Administrator");
            ViewBag.currentUser = User.Identity.GetUserId();


            return View(photo);

        }

        [Authorize]
        public ActionResult New()
        {
            Photo photo = new Photo();

            photo.Categories = GetAllCategories();
            photo.Albums = GetUserAlbums();

            // get the current users's id
            photo.UserId = User.Identity.GetUserId();
            
            return View(photo);

        }

        [HttpPost]
        [Authorize]
        [ValidateInput(false)]
        public ActionResult New(Photo photo)
        {
            photo.Categories = GetAllCategories();
            photo.Albums = GetUserAlbums();

            HttpPostedFileBase postedFile = Request.Files["ImageFile"];

            if (postedFile == null)
            {
                photo.Image = null;
                return View(photo);
            }

            //TO DO
            // Change image name so 2 users can add the same photo
            photo.Image = postedFile.FileName;
            var imagePath = HttpContext.Server.MapPath("~/Images/") + postedFile.FileName;

            try
            {
                if (ModelState.IsValid)
                {
                    postedFile.SaveAs(imagePath);
                    db.Photos.Add(photo);
                    db.SaveChanges();
                    TempData["message"] = "Photo was added";
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(photo);
                }
            }
            catch (Exception e)
            {
                return View(photo);
            }
        }

        [HttpDelete]
        [Authorize]
        public ActionResult Delete(int id)
        {
            Photo photo = db.Photos.Find(id);
            if (photo.UserId == User.Identity.GetUserId() ||
                User.IsInRole("Administrator"))
            {
                // TO DO
                // Remove photo from server
           
                db.Photos.Remove(photo);
                db.SaveChanges();
                TempData["message"] = "The photo was deleted";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "You are not allowed to delete another user's photo";
                return RedirectToAction("Index");
            }

        }

        [NonAction]
        private IEnumerable<SelectListItem> GetAllCategories()
        {
            // generam o lista goala
            var selectList = new List<SelectListItem>();

            // Extragem toate categoriile din baza de date
            var categories = from cat in db.Categories
                             select cat;

            // iteram prin categorii
            foreach (var category in categories)
            {
                // Adaugam in lista elementele necesare pentru dropdown
                selectList.Add(new SelectListItem
                {
                    Value = category.CategoryId.ToString(),
                    Text = category.CategoryName.ToString()
                });
            }

            // returnam lista de categorii
            return selectList;
        }

        [NonAction]
        private IEnumerable<SelectListItem> GetUserAlbums()
        {
            var selectLIst = new List<SelectListItem>();

            //get all user's albums

            var user = User.Identity.GetUserId();
            var albums = from al in db.Albums
                         where al.UserId == user
                         select al;

            foreach ( var album in albums)
            {
                // add albums to dropdown
                selectLIst.Add(new SelectListItem
                {
                    Value = album.Id.ToString(),
                    Text = album.AlbumName.ToString()
                });
                
            }
            return selectLIst;
        }
    }


}