using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MiniERPprojesi.Models;

namespace MiniERPprojesi.Controllers
{
    [Authorize(Roles = "Admin,Ürün ve Stok Sorumlusu")]
    public class KategoriController : Controller
    {
        MiniERPDBEntities6 db = new MiniERPDBEntities6();

        public ActionResult Index()
        {
            var kategoriler = db.Kategoriler.ToList();
            return View(kategoriler);
        }

        // AJAX: Ekle/Güncelle
        [HttpPost]
        public int Ekle(Kategoriler k)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(k.KategoriAdi))
                    return 2; // Eksik alan


                if (k.KategoriID == 0)
                {
                    db.Kategoriler.Add(k);
                }
                else // Güncelleme
                {
                    var mevcut = db.Kategoriler.FirstOrDefault(x => x.KategoriID == k.KategoriID);
                    if (mevcut == null)
                        return 1;

                    mevcut.KategoriAdi = k.KategoriAdi;
                }

                db.SaveChanges();
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        // AJAX: Sil
        [HttpPost]
        public int Sil(int id)
        {
            try
            {
                var silinecek = db.Kategoriler.FirstOrDefault(x => x.KategoriID == id);
                if (silinecek == null)
                    return 1;

                db.Kategoriler.Remove(silinecek);
                db.SaveChanges();
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        // AJAX: Güncelleme için veriyi getir
        [HttpGet]
        public JsonResult Getir(int id)
        {
            var k = db.Kategoriler.FirstOrDefault(x => x.KategoriID == id);
            if (k == null)
                return Json(null, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                k.KategoriID,
                k.KategoriAdi
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
