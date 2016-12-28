using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AdvAgen.Models;
using System.Data.Entity.Validation;
using Microsoft.AspNet.Identity;

namespace AdvAgen.Controllers
{
    public class AdvertisingController : Controller
    {
        private Entities db = new Entities();

        // GET: Advertising
        public ActionResult Index()
        {
            var advertisings = db.advertisings.Include(a => a.campaign);
            return View(advertisings.ToList());
        }

        // GET: Advertising/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            advertising advertising = db.advertisings.Find(id);
            if (advertising == null)
            {
                return HttpNotFound();
            }
            return View(advertising);
        }

        [Authorize(Roles = "Manager")]
        // GET: Advertising/Create
        public ActionResult Create()
        {
            ViewBag.campaignName = new SelectList(db.campaigns, "name", "name");
            return View();
        }

        // POST: Advertising/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "name,category,price,briefDescription,fullDescription,campaignName")] advertising advertising)
        {
            if (ModelState.IsValid)
            {
                db.advertisings.Add(advertising);
                try
                {
                    db.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (DbEntityValidationResult validationError in ex.EntityValidationErrors)
                    {
                        Response.Write("Object: " + validationError.Entry.Entity.ToString());
                        Response.Write("");
                        foreach (DbValidationError err in validationError.ValidationErrors)
                        {
                            Response.Write(err.ErrorMessage + "");
                        }
                    }
                }
                Logger.Log.Info("Пользователь" + User.Identity.GetUserId() + " удалил рекламу " + advertising.name);
                return RedirectToAction("Index");
            }

            ViewBag.campaignName = new SelectList(db.campaigns, "name", "name", advertising.campaignName);           
            return View(advertising);
        }

        [Authorize(Roles = "Manager")]
        // GET: Advertising/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            advertising advertising = db.advertisings.Find(id);
            if (advertising == null)
            {
                return HttpNotFound();
            }
            ViewBag.campaignName = new SelectList(db.campaigns, "name", "name", advertising.campaignName);
            return View(advertising);
        }

        // POST: Advertising/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "name,category,price,briefDescription,fullDescription,campaignName")] advertising advertising)
        {
            if (ModelState.IsValid)
            {
                db.Entry(advertising).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.campaignName = new SelectList(db.campaigns, "name", "name", advertising.campaignName);
            Logger.Log.Info("Пользователь" + User.Identity.GetUserId() + " изменил рекламу " + advertising.name);
            return View(advertising);
        }

        [Authorize(Roles = "Manager")]
        // GET: Advertising/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            advertising advertising = db.advertisings.Find(id);
            if (advertising == null)
            {
                return HttpNotFound();
            }
            return View(advertising);
        }

        // POST: Advertising/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            advertising advertising = db.advertisings.Find(id);
            db.advertisings.Remove(advertising);
            db.SaveChanges();
            Logger.Log.Info("Пользователь" + User.Identity.GetUserId() + " удалил рекламу " + advertising.name);
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
