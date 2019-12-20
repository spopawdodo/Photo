using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PhotoApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhotoApplication.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private ApplicationDbContext db = ApplicationDbContext.Create();

        // GET: Users
        public ActionResult Index()
        {
            var users = from user in db.Users
                        orderby user.UserName
                        select user;

            ViewBag.UserList = users;
            return View();
        }

        public ActionResult Show(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            user.AllRoles = GetAllRoles();
            ViewBag.utilizatorCurent = User.Identity.GetUserId();

            ViewBag.DisplayEditButton = false;
            if (User.Identity.GetUserId() == user.Id || User.IsInRole("Administrator"))
            {
                ViewBag.DisplayEditButton = true;
            }

            var roles = db.Roles.ToList();

            var roleName = roles.Where(j => j.Id ==
               user.Roles.FirstOrDefault().RoleId).
               Select(a => a.Name).FirstOrDefault();

            ViewBag.roleName = roleName;


            return View(user);
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            user.AllRoles = GetAllRoles();
            var roleName = user.Roles.FirstOrDefault();

            ViewBag.CanEditRole = false;
            if (User.IsInRole("Administrator"))
            {
                ViewBag.CanEditRole = true;
            }
            
            ViewBag.userRole = roleName.RoleId;
            return View(user);

        }


        [HttpPut]
        public ActionResult Edit(string id, ApplicationUser newData)
        {
            ApplicationUser user = db.Users.Find(id);
            // define all roles property in IdentityModels
            user.AllRoles = GetAllRoles();
            var userRole = user.Roles.FirstOrDefault();
            ViewBag.userRole = userRole.RoleId;

            try
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

                if (TryUpdateModel(user))
                {
                    user.UserName = newData.UserName;
                    user.Email = newData.Email;
                    user.PhoneNumber = newData.PhoneNumber;
                    if (User.IsInRole("Administrator"))
                    {
                        var roles = from role in db.Roles select role;

                        foreach (var role in roles)
                        {
                            UserManager.RemoveFromRole(id, role.Name);
                        }

                        var selectRole =
                            db.Roles.Find(HttpContext.Request.Params.Get("newRole"));
                        UserManager.AddToRole(id, selectRole.Name);
                    } 
                   
                    db.SaveChanges();
                }
                return RedirectToAction("Index");

            }
            catch (Exception e)
            {
                Response.Write(e.Message);
                return View(user);
            }
        }
        

        [HttpDelete]
        public ActionResult Delete(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            if (User.Identity.GetUserId() == id || User.IsInRole("Administrator"))
            {
                db.Users.Remove(user);
                db.SaveChanges();
            }
            return RedirectToAction("Index");

        }


        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            var selectList = new List<SelectListItem>();

            var roles = from role in db.Roles select role;

            foreach (var role in roles)
            {
                selectList.Add(new SelectListItem
                {
                    Value = role.Id.ToString(),
                    Text = role.Name.ToString()
                });
            }
            return selectList;
        }

    }
}