﻿using PhotoApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhotoApplication.Controllers
{
    
    public class CategoryController : Controller
    {
        //private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationDbContext db = ApplicationDbContext.Create();

        // GET: Category
        public ActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }


            var categories = from category in db.Categories
                             orderby category.CategoryName
                             select category;
            ViewBag.Categories = categories;
            return View();

        }
        

        public ActionResult Show(int id)
        {
            Category category = db.Categories.Find(id);
            var photos = db.Photos.Include("Category").Include("Album").Include("User").Where(p => p.CategoryId == id).OrderByDescending(p => p.Date);

            ViewBag.displayButtons = false;
            if (User.IsInRole("Administrator"))
            {
                ViewBag.displayButtons = true;
            }

            ViewBag.isAdmin = User.IsInRole("Administrator");
            ViewBag.Photos = photos;
            return View(category);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public ActionResult New(Category cat)
        {
            try
            {
                db.Categories.Add(cat);
                db.SaveChanges();
                //
                TempData["message"] = "Categoria a fost adaugata!";
                //
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View();
            }
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(int id)
        {
            Category category = db.Categories.Find(id);
            /*ViewBag.Category = category;
            return View();*/
            return View(category);
        }

        [HttpPut]
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(int id, Category requestCategory)
        {
            try
            {
                Category category = db.Categories.Find(id);
                if (TryUpdateModel(category))
                {
                    category.CategoryName = requestCategory.CategoryName;
                    //
                    TempData["message"] = "Categoria a fost modificata!";
                    //
                    db.SaveChanges();
                }

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View();
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(int id)
        {
            Category category = db.Categories.Find(id);
            db.Categories.Remove(category);
            //
            TempData["message"] = "Categoria a fost stearsa!";
            //
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}