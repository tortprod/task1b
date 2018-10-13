using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using NewTest.Models;

namespace NewTest.Controllers
{
    public class OrdersController : Controller
    {
        private TestEntities db = new TestEntities();
        private List<string[]> global = new List<string[]>();

        public ActionResult Index()
        {
            var orders = db.Orders.Include(o => o.Inventory);
            return View(orders.ToList());
        }

        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        public ActionResult Create()
        {
            ViewBag.InventId = new SelectList(db.Inventories, "Id", "Type");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Description,InventId,Count,Cost,Date")] Order order)
        {
            var inv = db.Inventories.Find(order.InventId);
            int price = inv.Cost;
            if (ModelState.IsValid)
            {
                order.Id = Guid.NewGuid();
                order.Date = DateTime.Now;
                order.Cost = price * order.Count;
                db.Orders.Add(order);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.InventId = new SelectList(db.Inventories, "Id", "Type", order.InventId);
            return View(order);
        }

        public ActionResult Report()
        {
            return View(global);
        }

        [HttpPost]
        public ActionResult Report(string str)
        {
            string[] tmp = str.Split(new char[] { ',', ' ', '.', '-' });
            DateTime start = new DateTime(Convert.ToInt16(tmp[1]), Convert.ToInt16(tmp[0]),1);
            DateTime end = new DateTime(Convert.ToInt16(tmp[1]), Convert.ToInt16(tmp[0]), DateTime.DaysInMonth(Convert.ToInt16(tmp[1]), Convert.ToInt16(tmp[0])));
            Dictionary<string, int> dict = new Dictionary<string, int>();
            List<string[]> inventList = new List<string[]>();
            IQueryable<Order> orders;
            orders = db.Orders.Where(x => x.Date >= start && x.Date <= end);


            foreach (var item in db.Inventories)
            {
                dict.Add(item.Type, 0);
            }

            foreach (var t in orders)
            {
                dict[t.Inventory.Type] += t.Count;
            }

            foreach (var item in dict)
            {
                inventList.Add(new string[] { item.Key, item.Value.ToString() });
            }
            global = inventList;
            return View(inventList);
        }

        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            ViewBag.InventId = new SelectList(db.Inventories, "Id", "Type", order.InventId);
            return View(order);
        }

        public ActionResult Edit([Bind(Include = "Id,Description,InventId,Count")] Order order)
        {
            var inv = db.Inventories.Find(order.InventId);
            int price = inv.Cost;
            if (ModelState.IsValid)
            {
                order.Cost = price * order.Count;
                order.Date = DateTime.Now;
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.InventId = new SelectList(db.Inventories, "Id", "Type", order.InventId);
            return View(order);
        }

        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Order order = db.Orders.Find(id);
            db.Orders.Remove(order);
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
