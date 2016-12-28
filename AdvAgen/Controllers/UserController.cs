using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AdvAgen.Models;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using System.Data.Entity.Validation;

namespace AdvAgen.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private Entities db = new Entities();

        // GET: User
        public ActionResult Index()
        {
            return View(db.AspNetUsers.ToList());
        }

        // GET: User/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUser aspNetUser = db.AspNetUsers.Find(id);
            if (aspNetUser == null)
            {
                return HttpNotFound();
            }
            return View(aspNetUser);
        }

        // POST: User/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName,roleId,AspNetRole,role")] AspNetUser aspNetUser)
        {
            if (ModelState.IsValid)
            {
                AspNetUser user = db.AspNetUsers.Where(p => p.Id == aspNetUser.Id).FirstOrDefault();
                String oldRole = user.AspNetRole.Name;                
                Roles.RemoveUserFromRole(aspNetUser.UserName, user.AspNetRole.Name);
                Roles.AddUserToRole(aspNetUser.UserName, aspNetUser.AspNetRole.Name);                
                user.roleId = db.AspNetRoles.Where(p => p.Name == aspNetUser.AspNetRole.Name).FirstOrDefault().Id;
                String role = aspNetUser.AspNetRole.Name;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                if (oldRole == "Customer")
                {
                    customer c = db.customers.Where(p => p.userId == user.Id).FirstOrDefault();
                    if (c != null)
                    {
                        db.orders.RemoveRange(c.orders.ToList());
                        db.customers.Remove(c);
                    }                    
                }
                if (role == "Customer")
                {
                    customer cus = new customer
                    {
                        AspNetUser = user,
                        userId = user.Id,
                        fio = "",
                        nickname = "",
                        phone = "",
                        id = db.customers.Max(p => p.id)+1,
                        registrationDate = DateTime.Today
                    };
                    cus.AspNetUser.roleId = user.roleId;
                    db.customers.Add(cus);
                    user.customers.Add(cus);
                    db.Entry(cus).State = EntityState.Added;
                }
                try
                {
                    db.SaveChanges();
                    Logger.Log.Info("Пользователь " + User.Identity.Name + " изменил информацию о пользователе " + aspNetUser.Email);
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (DbEntityValidationResult validationError in ex.EntityValidationErrors)
                    {
                        foreach (DbValidationError err in validationError.ValidationErrors)
                        {
                            Logger.Log.Error("Произошла ошибка при изменении роли у пользователя" + aspNetUser.Email + "Ошибка:" + err.ErrorMessage);
                        }
                    }
                }                
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        // GET: User/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUser aspNetUser = db.AspNetUsers.Find(id);
            if (aspNetUser == null)
            {
                return HttpNotFound();
            }
            return View(aspNetUser);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            AspNetUser aspNetUser = db.AspNetUsers.Find(id);
            db.orders.RemoveRange(aspNetUser.customers.First().orders.ToList());
            db.customers.Remove(aspNetUser.customers.First());
            db.AspNetUsers.Remove(aspNetUser);
            try
            {
                db.SaveChanges();
                Logger.Log.Info("Пользователь " + User.Identity.Name + " удалил пользователя " + aspNetUser.Email);
            }
            catch (DbEntityValidationException ex)
            {
                foreach (DbEntityValidationResult validationError in ex.EntityValidationErrors)
                {
                    foreach (DbValidationError err in validationError.ValidationErrors)
                    {
                        Logger.Log.Error("Произошла ошибка при удалении пользователя" + aspNetUser.Email + "Ошибка:" + err.ErrorMessage);
                    }
                }
            }            
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
