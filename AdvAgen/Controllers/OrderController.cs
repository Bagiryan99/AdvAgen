using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AdvAgen.Models;

namespace AdvAgen.Controllers
{
    public class OrderController : Controller
    {
        private Entities db = new Entities();   

        [Authorize(Roles = "Manager,Customer")]
        // GET: Order
        public ActionResult Index()
        {
            if(User.IsInRole("Manager"))
            {
                var orders = db.orders.Include(o => o.advertising).Include(o => o.customer);
                return View(orders.ToList());
            } else
            {
                AspNetUser user = db.AspNetUsers.Where(p => p.UserName == User.Identity.Name).First();
                customer cus = db.customers.Where(p => p.userId == user.Id).FirstOrDefault();
                var orders = db.orders.Where(p => p.customerId == cus.id).ToList();
                return View(orders);
            }                   
        }

        // GET: Order/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            order order = db.orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        [Authorize(Roles = "Customer")]
        // GET: Order/Create
        public ActionResult Create(String advertisingName)
        {
            order order = new order()
            {
                advertisingName = advertisingName
            };
            ViewBag.o = order;
            return View(order);
        }

        // POST: Order/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(order order)
        {
            if (ModelState.IsValid)
            {
                order.createdDate = DateTime.Today;
                AspNetUser user = db.AspNetUsers.Where(p => p.UserName == User.Identity.Name).First();
                order.customer = db.customers.Where(p => p.userId == user.Id).FirstOrDefault();
                order.customerId = order.customer.id;
                order.advertising = db.advertisings.Where(p => p.name == order.advertisingName).FirstOrDefault();
                order.number = db.orders.Max(p => p.number);
                order.statusId = 1;
                db.orders.Add(order);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(order);
        }
        [Authorize(Roles = "Manager")]
        // GET: Order/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            order order = db.orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            ViewBag.advertisingName = new SelectList(db.advertisings, "name", "category", order.advertisingName);
            ViewBag.customerId = new SelectList(db.customers, "id", "fio", order.customerId);
            return View(order);
        }

        // POST: Order/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "number,customerId,advertisingName,createdDate,status")] order order)
        {
            if (ModelState.IsValid)
            {
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.advertisingName = new SelectList(db.advertisings, "name", "category", order.advertisingName);
            ViewBag.customerId = new SelectList(db.customers, "id", "fio", order.customerId);
            return View(order);
        }
        [Authorize(Roles = "Manager,Customer")]
        // GET: Order/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            order order = db.orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: Order/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            order order = db.orders.Find(id);
            db.orders.Remove(order);
            db.SaveChanges();
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
