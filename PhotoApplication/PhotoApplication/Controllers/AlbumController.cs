using Microsoft.AspNet.Identity;
using Microsoft.Security.Application;
using PhotoApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhotoApplication.Controllers
{
    
    public class AlbumController : Controller
    {
        private ApplicationDbContext db = ApplicationDbContext.Create();

        private int _perPage = 12;

        // GET: Category
        public ActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            var albums = db.Albums.Include("User").OrderBy(a => a.Date);
            var totalAlbums = albums.Count();
            var currentPage = Convert.ToInt32(Request.Params.Get("page"));

            var offset = 0;
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * this._perPage;
            }

            var paginatedAlbums = albums.Skip(offset).Take(this._perPage);

            ViewBag.perPage = this._perPage;
            ViewBag.total = totalAlbums;
            ViewBag.lastPage = Math.Ceiling((float)totalAlbums / (float)this._perPage);
            ViewBag.Albums = paginatedAlbums;
            return View();

        }

        public ActionResult Show(int id)
        {
            Album album = db.Albums.Find(id);
            var photos = from photo in db.Photos
                         join cat in db.Categories on photo.CategoryId equals cat.CategoryId
                         where photo.AlbumId == id 
                         orderby photo.Date descending
                         select photo;

            ViewBag.isAdmin = User.IsInRole("Administrator");
            ViewBag.currentUser = User.Identity.GetUserId();
            ViewBag.Photos = photos;
            return View(album);
        }

        [Authorize]
        public ActionResult New()
        {
            Album album = new Album();  

            // Preluam ID-ul utilizatorului curent
            album.UserId = User.Identity.GetUserId();

            return View(album);

        }

        [HttpPost]
        [Authorize]
        [ValidateInput(false)]
        public ActionResult New(Album album)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Protect content from XSS
                    album.AlbumName = Sanitizer.GetSafeHtmlFragment(album.AlbumName);
                    db.Albums.Add(album);
                    db.SaveChanges();
                    TempData["message"] = "The album was created";
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(album);
                }
            }
            catch (Exception e)
            {
                return View(album);
            }
        }

        [Authorize]
        public ActionResult Edit(int id)
        {
            Album album = db.Albums.Find(id);
            ViewBag.Article = album;

            if (album.UserId == User.Identity.GetUserId() ||
                User.IsInRole("Administrator"))
            {
                return View(album);
            }
            else
            {
                TempData["message"] = "You are not allowed to modify another user's album!";
                return RedirectToAction("Index");
            }
        }


        [HttpPut]
        [Authorize]
        [ValidateInput(false)]
        public ActionResult Edit(int id, Album requestAlbum)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Album article = db.Albums.Find(id);
                    if (article.UserId == User.Identity.GetUserId() ||
                        User.IsInRole("Administrator"))
                    {
                        if (TryUpdateModel(article))
                        { 
                            // Protect content from XSS
                            requestAlbum.AlbumName = Sanitizer.GetSafeHtmlFragment(requestAlbum.AlbumName);
                            article.Date = requestAlbum.Date;
                            db.SaveChanges();
                            TempData["message"] = "The album has been updated";
                        }
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["message"] = "You are not allowed to modify another user's album!";
                        return RedirectToAction("Index");
                    }

                }
                else
                {
                    TempData["message"] = "ModelState not valid";
                    return View(requestAlbum);
                }

            }
            catch (Exception e)
            {
                TempData["message"] = "Exception!";
                return View(requestAlbum);
            }
        }


        [HttpDelete]
        [Authorize]
        public ActionResult Delete(int id)
        {
            Album album = db.Albums.Find(id);
            if (album.UserId == User.Identity.GetUserId() ||
                User.IsInRole("Administrator"))
            {
                db.Albums.Remove(album);
                // TO DO
                // delete all pictures from album 
                db.SaveChanges();
                TempData["message"] = "The album was deleted";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "You are not allowed to delete this album!";
                return RedirectToAction("Index");
            }

        }

    }

}