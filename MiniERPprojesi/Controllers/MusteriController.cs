using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MiniERPprojesi.Models;

namespace MiniERPprojesi.Controllers
{
    [Authorize(Roles = "Admin,Siparis ve Faturalama Sorumlusu")]
    public class MusteriController : Controller
    {
        MiniERPDBEntities6 db = new MiniERPDBEntities6();

        // View'a v_musteriler listesini gönderir
        public ActionResult Index(string musteriTuru, string siralama)
        {
            ViewBag.iller = db.Iller.ToList();

            var musteriler = db.v_musteriler.AsQueryable();

            if (!string.IsNullOrEmpty(musteriTuru))
            {
                musteriler = musteriler.Where(x => x.MusteriTuru == musteriTuru);
            }

            // Sıralama
            if (siralama == "az")
            {
                musteriler = musteriler.OrderBy(x => x.Ad);
            }
            else if (siralama == "za")
            {
                musteriler = musteriler.OrderByDescending(x => x.Ad);
            }

            ViewBag.SeciliMusteriTuru = musteriTuru;
            ViewBag.SeciliSiralama = siralama;

            return View(musteriler.ToList());
        }


        // AJAX: İlçeleri getir
        [HttpGet]
        public JsonResult IlceleriGetir(int ilID)
        {
            var ilceler = db.Ilceler
                .Where(x => x.IlID == ilID)
                .Select(x => new { x.IlceID, x.IlceAdi })
                .ToList();

            return Json(ilceler, JsonRequestBehavior.AllowGet);
        }

        // AJAX: Müşteri ekle/güncelle
        [HttpPost]
        public int Ekle(Musteriler m, Adresler a)
        {
            var sid = Session["kullaniciID"]?.ToString();
            int kullaniciId;
            if (!int.TryParse(sid, out kullaniciId)) return -2;

            try
            {
                if (string.IsNullOrWhiteSpace(m.Ad) ||
                    string.IsNullOrWhiteSpace(m.Soyad) ||
                    string.IsNullOrWhiteSpace(m.Email) ||
                    string.IsNullOrWhiteSpace(m.Telefon) ||
                    string.IsNullOrWhiteSpace(m.KimlikNo) ||
                    string.IsNullOrWhiteSpace(m.MusteriTuru) ||
                    string.IsNullOrWhiteSpace(a.Mahalle) ||
                    string.IsNullOrWhiteSpace(a.Sokak) ||
                    string.IsNullOrWhiteSpace(a.BinaNo) ||
                    a.IlceID <= 0)
                    return 2;

                if (m.MusteriID == 0) // EKLE
                {
                    db.Adresler.Add(a);
                    db.SaveChanges();

                    m.AdresID = a.AdresID;
                    m.OlusturanKullaniciID = kullaniciId;
                    db.Musteriler.Add(m);
                }
                else // GÜNCELLE
                {
                    var mevcutMusteri = db.Musteriler.FirstOrDefault(x => x.MusteriID == m.MusteriID);
                    if (mevcutMusteri == null) return 1;

                    var mevcutAdres = db.Adresler.FirstOrDefault(x => x.AdresID == m.AdresID);
                    if (mevcutAdres == null) return 1;

                    mevcutMusteri.Ad = m.Ad;
                    mevcutMusteri.Soyad = m.Soyad;
                    mevcutMusteri.Email = m.Email;
                    mevcutMusteri.Telefon = m.Telefon;
                    mevcutMusteri.KimlikNo = m.KimlikNo;
                    mevcutMusteri.MusteriTuru = m.MusteriTuru;
                    mevcutMusteri.OlusturanKullaniciID = kullaniciId;

                    mevcutAdres.IlceID = a.IlceID;
                    mevcutAdres.Mahalle = a.Mahalle;
                    mevcutAdres.Sokak = a.Sokak;
                    mevcutAdres.BinaNo = a.BinaNo;
                    mevcutAdres.DaireNo = a.DaireNo;
                    mevcutAdres.PostaKodu = a.PostaKodu;
                }

                db.SaveChanges();
                return 0;
            }
            catch { return -1; }
        }


        // AJAX: Müşteri silme
        [HttpPost]
        public int Sil(int id)
        {
            try
            {
                var m = db.Musteriler.FirstOrDefault(x => x.MusteriID == id);
                if (m == null)
                    return 1;

                var a = db.Adresler.FirstOrDefault(x => x.AdresID == m.AdresID);
                if (a != null)
                    db.Adresler.Remove(a);

                db.Musteriler.Remove(m);
                db.SaveChanges();
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        // AJAX: Güncelleme için müşteri ve adres bilgilerini getir
        [HttpGet]
        public JsonResult MusteriGetir(int id)
        {
            var m = db.Musteriler
                .Where(x => x.MusteriID == id)
                .Select(x => new
                {
                    x.MusteriID,
                    x.Ad,
                    x.Soyad,
                    x.Email,
                    x.Telefon,
                    x.KimlikNo,
                    x.MusteriTuru,
                    x.OlusturanKullaniciID,
                    x.AdresID,
                    Adres = new
                    {
                        x.Adresler.Mahalle,
                        x.Adresler.Sokak,
                        x.Adresler.BinaNo,
                        x.Adresler.DaireNo,
                        x.Adresler.PostaKodu,
                        x.Adresler.IlceID,
                        IlID = x.Adresler.Ilceler.IlID
                    }
                })
                .FirstOrDefault();

            return Json(m, JsonRequestBehavior.AllowGet);
        }
    }
}
