using MiniERPprojesi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;

namespace MiniERPprojesi.Controllers
{
    [Authorize(Roles = "Admin,Ürün ve Stok Sorumlusu")]
    public class DepoController : Controller
    {
        MiniERPDBEntities6 db = new MiniERPDBEntities6();

        // View'a verilerle birlikte depoları gönderir
        public ActionResult Index()
        {
            ViewBag.iller = db.Iller.ToList();
            List<v_depolar> depolar = db.v_depolar.ToList();
            return View(depolar);
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

        // AJAX: Depo ekle/güncelle
        [HttpPost]
        public int Ekle(Depolar d, Adresler a)
        {
            try
            {
                var sid = Session["kullaniciID"]?.ToString();
                int kullaniciId;
                if (!int.TryParse(sid, out kullaniciId))
                {
                    return -2; // Oturum yok
                }

                if (string.IsNullOrWhiteSpace(d.DepoAdi) ||
                    a.IlceID <= 0 ||
                    string.IsNullOrWhiteSpace(a.Mahalle) ||
                    string.IsNullOrWhiteSpace(a.Sokak) ||
                    string.IsNullOrWhiteSpace(a.BinaNo) ||
                    string.IsNullOrWhiteSpace(a.PostaKodu))
                {
                    return 2; // Eksik alan
                }

                if (d.DepoID == 0) // Ekleme
                {
                    db.Adresler.Add(a);
                    db.SaveChanges();

                    d.AdresID = a.AdresID;
                    d.OlusturanKullaniciID = kullaniciId;
                    db.Depolar.Add(d);
                }
                else // Güncelleme
                {
                    Depolar mevcutDepo = db.Depolar.FirstOrDefault(x => x.DepoID == d.DepoID);
                    if (mevcutDepo == null)
                        return 1;

                    Adresler mevcutAdres = db.Adresler.FirstOrDefault(x => x.AdresID == d.AdresID);
                    if (mevcutAdres == null)
                        return 1;

                    // Depo bilgileri
                    mevcutDepo.DepoAdi = d.DepoAdi;
                    mevcutDepo.OlusturanKullaniciID = kullaniciId;

                    // Adres bilgileri
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
            catch
            {
                return -1;
            }
        }


        // AJAX: Depo silme
        [HttpPost]
        public int Sil(int id)
        {
            try
            {
                Depolar d = db.Depolar.FirstOrDefault(x => x.DepoID == id);
                if (d == null)
                    return 1;

                Adresler a = db.Adresler.FirstOrDefault(x => x.AdresID == d.AdresID);
                if (a != null)
                    db.Adresler.Remove(a);

                db.Depolar.Remove(d);
                db.SaveChanges();
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        // AJAX: Güncelleme için depo ve adres bilgilerini getir
        [HttpGet]
        public JsonResult DepoGetir(int id)
        {
            var d = db.Depolar
                .Where(x => x.DepoID == id)
                .Select(x => new
                {
                    x.DepoID,
                    x.DepoAdi,
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

            return Json(d, JsonRequestBehavior.AllowGet);
        }
    }
}
