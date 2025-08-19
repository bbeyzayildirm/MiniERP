using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MiniERPprojesi.Models;

namespace MiniERPprojesi.Controllers
{
    [Authorize(Roles = "Admin,Siparis ve Faturalama Sorumlusu")]
    public class FaturaController : Controller
    {
        MiniERPDBEntities6 db = new MiniERPDBEntities6();

        // View: Fatura listesi
        public ActionResult Index(string odemeDurumu, DateTime? baslangic, DateTime? bitis)
        {
            ViewBag.Siparisler = db.Siparisler
               .Where(s => s.SiparisDurumu != "Iptal" && s.FaturalandiMi == false)
               .Select(s => new SelectListItem
               {
                   Value = s.SiparisID.ToString(),
                   Text = s.SiparisNo
               }).ToList();

            var faturalar = db.v_faturalar.AsQueryable();

            if (!string.IsNullOrEmpty(odemeDurumu))
                faturalar = faturalar.Where(f => f.OdemeDurumu == odemeDurumu);

            if (baslangic.HasValue)
                faturalar = faturalar.Where(f => f.FaturaTarihi >= baslangic.Value);

            if (bitis.HasValue)
                faturalar = faturalar.Where(f => f.FaturaTarihi <= bitis.Value);

            ViewBag.SeciliDurum = odemeDurumu;
            ViewBag.Baslangic = baslangic?.ToString("yyyy-MM-dd");
            ViewBag.Bitis = bitis?.ToString("yyyy-MM-dd");

            return View(faturalar.ToList());
        }

        // AJAX: Fatura detaylarını getir (Sipariş detayları üzerinden)
        [HttpGet]
        public JsonResult FaturaDetayGetir(int id)
        {
            var fatura = db.Faturalar
                .Where(f => f.FaturaID == id)
                .Select(f => new
                {
                    f.FaturaID,
                    f.FaturaNo,
                    f.FaturaTarihi,
                    f.OdemeDurumu,
                    f.SiparisID,
                    f.Siparisler.SiparisNo
                })
                .FirstOrDefault();

            if (fatura == null)
                return Json(new { hata = "Fatura bulunamadı." }, JsonRequestBehavior.AllowGet);

            // Sipariş detaylarını getir
            var detaylar = db.v_siparisdetay
                .Where(x => x.SiparisID == fatura.SiparisID)
                .Select(x => new
                {
                    // Fatura bilgileri
                    fatura.FaturaNo,
                    fatura.FaturaTarihi,
                    fatura.OdemeDurumu,
                    fatura.SiparisNo,

                    // Müşteri bilgileri
                    x.MusteriAdi,
                    x.Email,
                    x.MusteriTuru,

                    // Adres bilgileri
                    x.Adres,
                    x.IlAdi,
                    x.IlceAdi,

                    // Ürün detayları
                    x.UrunAdi,
                    x.Adet,
                    x.BirimFiyat,
                    x.IptalEdildi
                })
                .ToList();

            return Json(detaylar, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public int FaturaEkle(Faturalar f)
        {
            try
            {
                var siparis = db.Siparisler.Find(f.SiparisID);
                if (siparis == null || siparis.FaturalandiMi == true)
                    return 1;

                var kalemler = db.SiparisKalemleri
                    .Where(k => k.SiparisID == f.SiparisID && k.IptalEdildi == false)
                    .ToList();

                if (kalemler.Count == 0)
                    return 2;

                var sid = Session["kullaniciID"]?.ToString();
                int kullaniciId;
                if (!int.TryParse(sid, out kullaniciId))
                    return -2; // Oturum yok

                f.KullaniciID = kullaniciId;

                // Toplam tutar
                f.ToplamTutar = kalemler.Sum(k => k.Adet * k.BirimFiyat);
                f.FaturaTarihi = DateTime.Now;
                f.FaturaNo = YeniFaturaNoUret();

                db.Faturalar.Add(f);

                // Siparişi işaretle
                siparis.FaturalandiMi = true;

                db.SaveChanges();
                return 0;
            }
            catch (Exception ex)
            {
                string detay = "";

                if (ex.InnerException != null)
                {
                    detay = ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null)
                        detay += " --> " + ex.InnerException.InnerException.Message;
                }
                else
                {
                    detay = ex.Message;
                }

                System.Diagnostics.Debug.WriteLine("Fatura Ekleme Hatası (detay): " + detay);
                return -1;
            }
        }

        [HttpGet]
        public JsonResult SiparisGetir(int id)
        {
            var s = db.Siparisler
               .Include("Musteriler")
               .Include("Adresler.Ilceler.Iller")
               .Where(x => x.SiparisID == id)
               .Select(x => new
               {
                   x.SiparisID,
                   x.SiparisNo,
                   x.FaturalandiMi,
                   x.MusteriID,
                   x.AdresID,
                   MusteriAdi = x.Musteriler.Ad + " " + x.Musteriler.Soyad,
                   MusteriTuru = x.Musteriler.MusteriTuru,
                   Email = x.Musteriler.Email,
                   Adres = new
                   {
                       x.Adresler.Mahalle,
                       x.Adresler.Sokak,
                       x.Adresler.BinaNo,
                       x.Adresler.DaireNo,
                       x.Adresler.PostaKodu,
                       IlceAdi = x.Adresler.Ilceler.IlceAdi,
                       IlAdi = x.Adresler.Ilceler.Iller.IlAdi
                   }
               })
               .FirstOrDefault();

            return Json(s, JsonRequestBehavior.AllowGet);
        }

        // AJAX: Faturanın ödeme durumu güncellenir
        [HttpPost]
        public int OdemeDurumuGuncelle(int id, string durum)
        {
            try
            {
                var f = db.Faturalar.Find(id);
                if (f == null) return 1; // Fatura yok

                f.OdemeDurumu = durum; // Yeni durum atanır
                db.SaveChanges();
                return 0; // Başarılı
            }
            catch
            {
                return -1; // Hata
            }
        }

        private string YeniFaturaNoUret()
        {
            string prefix = "FTR";
            string yil = DateTime.Now.Year.ToString();

            int yilIciSayac = db.Faturalar
                .Count(f => f.FaturaTarihi.Year == DateTime.Now.Year);

            string faturaNo;
            int deneme = 1;

            do
            {
                int siradaki = yilIciSayac + deneme;
                faturaNo = $"{prefix}{yil}{siradaki:D3}"; // FTR2025001
                deneme++;
            }
            while (db.Faturalar.Any(f => f.FaturaNo == faturaNo)); // Benzersiz olmalı

            return faturaNo;
        }



    }
}
