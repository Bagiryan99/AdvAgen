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
    public class CampaignController : Controller
    {
        private Entities db = new Entities();

        // GET: Campaign
        public ActionResult Index()
        {
            return View(db.campaigns.ToList());
        }

        [Authorize(Roles = "Manager")]
        // GET: Campaign/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Campaign/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "name,about,startDate,endDate")] campaign campaign)
        {
            if (ModelState.IsValid)
            {
                db.campaigns.Add(campaign);
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
                Logger.Log.Info("Пользователь" + User.Identity.GetUserId() + " создал акцию " + campaign.name);
                return RedirectToAction("Index");
            }
            return View(campaign);
        }
        [Authorize(Roles = "Manager")]
        // GET: Campaign/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            campaign campaign = db.campaigns.Find(id);
            if (campaign == null)
            {
                return HttpNotFound();
            }
            return View(campaign);
        }

        // POST: Campaign/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "name,about,startDate,endDate")] campaign campaign)
        {
            if (ModelState.IsValid)
            {
                db.Entry(campaign).State = EntityState.Modified;
                db.SaveChanges();
                Logger.Log.Info("Пользователь" + User.Identity.GetUserId() + " отредактировал акцию " + campaign.name);
                return RedirectToAction("Index");
            }
            return View(campaign);
        }
        [Authorize(Roles = "Manager")]
        // GET: Campaign/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            campaign campaign = db.campaigns.Find(id);
            if (campaign == null)
            {
                return HttpNotFound();
            }
            return View(campaign);
        }

        // POST: Campaign/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            campaign campaign = db.campaigns.Find(id);
            db.campaigns.Remove(campaign);
            db.SaveChanges();
            Logger.Log.Info("Пользователь" + User.Identity.GetUserId() + " удалил акцию " + campaign.name);
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
