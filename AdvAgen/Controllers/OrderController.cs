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
                Logger.Log.Info("Пользователь" + User.Identity.GetUserId() + " создал заказ №" + order.number);
                return RedirectToAction("Index");
            }
            return View(order);
        }
        [Authorize(Roles = "Manager,Customer")]
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
            return View(order);
        }

        // POST: Order/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "number,status")] order order)
        {
            if (ModelState.IsValid)
            {
                order o = db.orders.Where(p => p.number == order.number).FirstOrDefault();
                o.status = db.statuses.Where(p => p.name == order.status.name).FirstOrDefault();
                switch (o.status.Id)
                {
                    case 1:
                        {
                            o.statusId = o.status.Id;
                            db.Entry(o).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        break;
                    case 2:
                        {
                            o.statusId = o.status.Id;
                            db.Entry(o).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        break;
                    case 3:
                        {
                            order or = db.orders.Where(p => p.number == o.number).FirstOrDefault();
                            db.orders.Remove(or);
                            db.Entry(o).State = EntityState.Deleted;
                            db.SaveChanges();
                        }
                        break;
                    case 4:
                        {
                            order or = db.orders.Where(p => p.number == o.number).FirstOrDefault();
                            db.orders.Remove(or);
                            db.Entry(o).State = EntityState.Deleted;
                            db.SaveChanges();
                        }
                        break;
                    case 5:
                        {
                            o.statusId = o.status.Id+1;
                            db.Entry(o).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        break;
                }
                Logger.Log.Info("Пользователь" + User.Identity.GetUserId() + " изменил состояние заказа №" + order.number);
                return RedirectToAction("Index");
            }
            return View(order);
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
