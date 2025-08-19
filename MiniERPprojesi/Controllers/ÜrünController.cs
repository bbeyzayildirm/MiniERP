using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MiniERPprojesi.Models;
using System.Data.Entity.Migrations;

namespace MiniERPprojesi.Controllers
{
    [Authorize(Roles = "Admin,Ürün ve Stok Sorumlusu,Siparis ve Faturalama Sorumlusu")]
    public class UrunController : Controller
    {
        MiniERPDBEntities6 db = new MiniERPDBEntities6();

        // Listeleme ve ViewBag ile kategori gönderme
        [Authorize(Roles = "Admin,Ürün ve Stok Sorumlusu")]
        public ActionResult Index(int? kategoriId)
        {
            ViewBag.kategoriler = db.Kategoriler.ToList(); // Dropdown için

            List<v_urunler> urunler;

            if (kategoriId.HasValue)
            {
                urunler = db.v_urunler.Where(u => u.KategoriID == kategoriId.Value).ToList();
            }
            else
            {
                urunler = db.v_urunler.ToList(); // Tümünü göster
            }

            ViewBag.SeciliKategori = kategoriId; // Dropdownda seçili kalması için
            return View(urunler);
        }

        // Sipariş/Faturalama rolü için sade ürün listesi döndüren ayrı bir view
        [Authorize(Roles = "Admin,Ürün ve Stok Sorumlusu,Siparis ve Faturalama Sorumlusu")]
        public ActionResult IndexSiparisFaturalama(int? kategoriId)
        {
            ViewBag.Kategoriler = db.Kategoriler.Select(k => new SelectListItem
            {
                Value = k.KategoriID.ToString(),
                Text = k.KategoriAdi
            }).ToList();

            List<v_urunler> urunler;

            if (kategoriId.HasValue)
            {
                urunler = db.v_urunler.Where(u => u.KategoriID == kategoriId.Value).ToList();
            }
            else
            {
                urunler = db.v_urunler.ToList();
            }

            ViewBag.SeciliKategori = kategoriId;
            return View("IndexSiparisFaturalama", urunler);
        }

        [Authorize(Roles = "Admin,Ürün ve Stok Sorumlusu")]
        // Ürün ekleme ve güncelleme işlemi – resim destekli
        [HttpPost]
        public JsonResult UrunEkle(Urunler u, HttpPostedFileBase Resim)
        {
            var sid = Session["kullaniciID"]?.ToString();
            int currentUserId;
            if (!int.TryParse(sid, out currentUserId))
                return Json(-2); // Oturum yok

            try
            {
                if (string.IsNullOrWhiteSpace(u.UrunAdi) ||
           u.KategoriID <= 0 ||
           u.Fiyat <= 0) // Fiyat 0 veya eksi olamaz
                {
                    return Json(2); // Eksik alan
                }

                // Eğer formdan resim geldiyse kaydet
                if (Resim != null && Resim.ContentLength > 0)
                {
                    string dosyaAdi = Path.GetFileName(Resim.FileName);
                    string klasor = "~/Uploads/UrunResimleri/";
                    string yol = Path.Combine(Server.MapPath(klasor), dosyaAdi); // Fiziksel yol oluştur

                    // Klasör yoksa oluştur (ilk sefer için)
                    Directory.CreateDirectory(Server.MapPath(klasor));

                    // Resmi belirtilen dizine kaydet
                    Resim.SaveAs(yol);

                    // Veritabanına sanal yolu yaz
                    u.ResimYolu = klasor + dosyaAdi;
                }

                u.OlusturanKullaniciID = currentUserId;

                // UrunID varsa güncelle, yoksa ekle (tek metotta çözüm)
                db.Urunler.AddOrUpdate(x => x.UrunID, u);
                db.SaveChanges();

                return Json(0); // İşlem başarılı
            }
            catch
            {
                return Json(1); // Hata olursa (frontend kontrol eder)
            }
        }

        [Authorize(Roles = "Admin,Ürün ve Stok Sorumlusu")]
        [HttpPost]
        public JsonResult UrunSil(int id)
        {
            try
            {
                var silinecek = db.Urunler.FirstOrDefault(x => x.UrunID == id);
                if (silinecek != null)
                {
                    db.Urunler.Remove(silinecek);
                    db.SaveChanges();
                    return Json(0);
                }

                return Json(1);
            }
            catch
            {
                return Json(1);
            }
        }

        [Authorize(Roles = "Admin,Ürün ve Stok Sorumlusu")]
        // Güncelleme için ürün getir (JSON)
        [HttpGet]
        public JsonResult UrunGetir(int id)
        {
            var u = db.Urunler.Where(x => x.UrunID == id)
                              .Select(x => new {
                                  x.UrunID,
                                  x.UrunAdi,
                                  x.Aciklama,
                                  x.Fiyat,
                                  x.KategoriID
                              }).FirstOrDefault();

            return Json(u, JsonRequestBehavior.AllowGet);
        }

    }
}
