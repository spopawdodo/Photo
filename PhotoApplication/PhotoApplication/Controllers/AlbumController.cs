using Microsoft.AspNet.Identity;
using PhotoApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhotoApplication.Controllers
{
    [Authorize(Roles = "User, Administrator")]
    public class AlbumController : Controller
    {
        
        private ApplicationDbContext db = ApplicationDbContext.Create();

        private int _perPage = 10;

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

            ViewBag.isAdmin = User.IsInRole("Administrator");
            ViewBag.currentUser = User.Identity.GetUserId();

            return View(album);
        }
        
    }
}