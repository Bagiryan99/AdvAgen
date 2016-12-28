using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AdvAgen.Models;
using Microsoft.AspNet.Identity;

namespace AdvAgen.Controllers
{
    public class CustomerController : Controller
    {
        private Entities db = new Entities();

        // GET: Customer
        public ActionResult Index()
        {
            var customers = db.customers.Include(c => c.AspNetUser);
            return View(customers.ToList());
        }

        // GET: Customer/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            customer customer = db.customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }        

        // GET: Customer/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            customer customer = db.customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            ViewBag.userId = new SelectList(db.AspNetUsers, "Id", "Email", customer.userId);
            return View(customer);
        }

        // POST: Customer/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,fio,registrationDate,phone,userId,AspNetUser")] customer customer)
        {
            if (ModelState.IsValid)
            {
                String username = customer.AspNetUser.UserName;
                customer.AspNetUser = db.AspNetUsers.Where(p => p.Id == customer.userId).First();
                customer.AspNetUser.UserName = username;
                db.Entry(customer).State = EntityState.Modified;
                db.SaveChanges();
                Logger.Log.Info("Пользователь" + User.Identity.GetUserId() + " отредактировал информацию о клиенте " + customer.fio);
                return RedirectToAction("Index");
            }
            ViewBag.userId = new SelectList(db.AspNetUsers, "Id", "Email", customer.userId);
            return View(customer);
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
