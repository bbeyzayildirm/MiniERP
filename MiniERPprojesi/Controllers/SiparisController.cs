using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MiniERPprojesi.Models;

namespace MiniERPprojesi.Controllers
{
    [Authorize(Roles = "Admin,Siparis ve Faturalama Sorumlusu")]
    public class SiparisController : Controller
    {
        MiniERPDBEntities6 db = new MiniERPDBEntities6();

        public ActionResult Index(string siparisDurumu, bool? faturalandiMi, DateTime? baslangic, DateTime? bitis)
        {
            ViewBag.Musteriler = db.Musteriler.Select(m => new SelectListItem
            {
                Value = m.MusteriID.ToString(),
                Text = m.Ad + " " + m.Soyad
            }).ToList();

            ViewBag.iller = db.Iller.Select(i => new SelectListItem
            {
                Value = i.IlID.ToString(),
                Text = i.IlAdi
            }).ToList();

            ViewBag.ilceler = db.Ilceler.Select(i => new SelectListItem
            {
                Value = i.IlceID.ToString(),
                Text = i.IlceAdi
            }).ToList();

            ViewBag.Urunler = db.Urunler.Select(u => new SelectListItem
            {
                Value = u.UrunID.ToString(),
                Text = u.UrunAdi
            }).ToList();

            ViewBag.Depolar = db.Depolar.Select(d => new SelectListItem
            {
                Value = d.DepoID.ToString(),
                Text = d.DepoAdi
            }).ToList();

            ViewBag.SiparisDurumlari = new List<SelectListItem>
    {
        new SelectListItem { Text = "Hazırlanıyor", Value = "Hazirlaniyor" },
        new SelectListItem { Text = "Teslim Edildi", Value = "Teslim Edildi" },
        new SelectListItem { Text = "İptal", Value = "Iptal" }
    };

            ViewBag.SeciliDurum = siparisDurumu;
            ViewBag.SeciliFatura = faturalandiMi;
            ViewBag.Baslangic = baslangic?.ToString("yyyy-MM-dd");
            ViewBag.Bitis = bitis?.ToString("yyyy-MM-dd");

            var siparisler = db.v_siparisler.AsQueryable();

            if (!string.IsNullOrEmpty(siparisDurumu))
                siparisler = siparisler.Where(s => s.SiparisDurumu == siparisDurumu);

            if (faturalandiMi.HasValue)
                siparisler = siparisler.Where(s => s.FaturalandiMi == faturalandiMi.Value);

            if (baslangic.HasValue)
                siparisler = siparisler.Where(s => s.SiparisTarihi >= baslangic.Value);

            if (bitis.HasValue)
                siparisler = siparisler.Where(s => s.SiparisTarihi <= bitis.Value);

            return View(siparisler.ToList());
        }

        [HttpGet]
        public JsonResult IlceleriGetir(int ilID)
        {
            var ilceler = db.Ilceler
                .Where(x => x.IlID == ilID)
                .Select(x => new { x.IlceID, x.IlceAdi })
                .ToList();

            return Json(ilceler, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult MusteriAdresGetir(int musteriID)
        {
            var adres = db.Musteriler
                .Where(x => x.MusteriID == musteriID)
                .Select(x => new
                {
                    x.Adresler.Mahalle,
                    x.Adresler.Sokak,
                    x.Adresler.BinaNo,
                    x.Adresler.DaireNo,
                    x.Adresler.PostaKodu,
                    x.Adresler.IlceID,
                    IlID = x.Adresler.Ilceler.IlID
                })
                .FirstOrDefault();

            return Json(adres, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult UrunFiyatGetir(int urunID)
        {
            var fiyat = db.Urunler
                .Where(u => u.UrunID == urunID)
                .Select(u => u.Fiyat)
                .FirstOrDefault();

            return Json(fiyat, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public int SiparisEkle(Siparisler siparis, Adresler adres, List<SiparisKalemleri> urunler)
        {
            // Oturumdan kullanıcıyı güvenli al
            var sid = Session["kullaniciID"]?.ToString();
            int currentUserId;
            if (!int.TryParse(sid, out currentUserId))
                return -2; // oturum yok/geçersiz


            try
            {
                if (siparis.MusteriID == 0 || string.IsNullOrEmpty(siparis.SiparisDurumu))
                    return 3;

                if (string.IsNullOrWhiteSpace(siparis.SiparisNo))
                {
                    siparis.SiparisNo = YeniSiparisNoUret();
                }


                if (adres.IlceID == 0 || string.IsNullOrWhiteSpace(adres.Mahalle) || string.IsNullOrWhiteSpace(adres.Sokak) || string.IsNullOrWhiteSpace(adres.BinaNo))
                    return 4;

                if (urunler == null || urunler.Count == 0)
                    return 2;

                foreach (var kalem in urunler)
                {
                    if (kalem.UrunID == 0 || kalem.Adet <= 0 || kalem.BirimFiyat <= 0 || kalem.DepoID == 0)
                        return 5;
                }

                siparis.KullaniciID = currentUserId;


                if (siparis.SiparisID == 0) // EKLE
                {
                    // Önce stok yeterli mi kontrol et
                    foreach (var kalem in urunler)
                    {
                        var stok = db.DepoStok.FirstOrDefault(s => s.DepoID == kalem.DepoID && s.UrunID == kalem.UrunID);
                        if (stok == null)
                            return 7;
                        if (stok.Miktar < kalem.Adet)
                            return 6;
                    }

                    // Adres kontrolü
                    var mevcutAdres = db.Adresler.FirstOrDefault(a =>
                        a.IlceID == adres.IlceID &&
                        a.Mahalle == adres.Mahalle &&
                        a.Sokak == adres.Sokak &&
                        a.BinaNo == adres.BinaNo &&
                        a.DaireNo == adres.DaireNo &&
                        a.PostaKodu == adres.PostaKodu
                    );

                    if (mevcutAdres != null)
                        siparis.AdresID = mevcutAdres.AdresID;
                    else
                    {
                        db.Adresler.Add(adres);
                        db.SaveChanges();
                        siparis.AdresID = adres.AdresID;
                    }

                    siparis.SiparisTarihi = DateTime.Now;
                    siparis.SiparisDurumu = "Hazirlaniyor";
                    db.Siparisler.Add(siparis);
                    db.SaveChanges();


                    // Stok düş ve kalemleri ekle
                    foreach (var kalem in urunler)
                    {
                        kalem.SiparisID = siparis.SiparisID;
                        db.SiparisKalemleri.Add(kalem);

                        var stok = db.DepoStok.FirstOrDefault(s => s.DepoID == kalem.DepoID && s.UrunID == kalem.UrunID);
                        stok.Miktar -= kalem.Adet;
                    }
                    db.SaveChanges();

                    // Stok hareketleri kaydı
                    foreach (var kalem in db.SiparisKalemleri.Where(x => x.SiparisID == siparis.SiparisID).ToList())
                    {
                        db.StokHareketleri.Add(new StokHareketleri
                        {
                            UrunID = kalem.UrunID,
                            DepoID = kalem.DepoID,
                            KullaniciID = siparis.KullaniciID,
                            SiparisKalemID = kalem.SiparisKalemID,
                            Miktar = kalem.Adet,
                            BirimFiyat = kalem.BirimFiyat,
                            HareketTipi = 2, // Çıkış
                            HareketTarihi = DateTime.Now,
                            IptalKaynakli = false
                        });
                    }

                    db.SaveChanges();
                }
                else // GÜNCELLE
                {
                    // Faturalanmışsa güncelleme yapılmasın
                    bool faturalandiMi = db.Siparisler
                        .Where(s => s.SiparisID == siparis.SiparisID)
                        .Select(s => s.FaturalandiMi)
                        .FirstOrDefault();

                    if (faturalandiMi)
                        return 9; // Faturalı sipariş, güncellenemez

                    var mevcutSiparis = db.Siparisler.FirstOrDefault(x => x.SiparisID == siparis.SiparisID);
                    if (mevcutSiparis == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Sipariş bulunamadı! Gelen ID: {siparis.SiparisID}");
                        return 1;
                    }

                    var eskiKalemler = db.SiparisKalemleri.Where(x => x.SiparisID == siparis.SiparisID).ToList();

                    // Yeni kalemler stokta var mı kontrol et (iade edilenlerle birlikte)
                    foreach (var kalem in urunler)
                    {
                        var stok = db.DepoStok.FirstOrDefault(s => s.DepoID == kalem.DepoID && s.UrunID == kalem.UrunID);
                        var eskiKalem = eskiKalemler.FirstOrDefault(x => x.UrunID == kalem.UrunID && x.DepoID == kalem.DepoID);
                        int iadeMiktar = eskiKalem != null ? eskiKalem.Adet : 0;

                        if (stok == null)
                            return 7;

                        if ((stok.Miktar + iadeMiktar) < kalem.Adet)
                            return 6;
                    }

                    // Adres güncellemesi
                    var ayniAdres = db.Adresler.FirstOrDefault(a =>
                        a.IlceID == adres.IlceID &&
                        a.Mahalle == adres.Mahalle &&
                        a.Sokak == adres.Sokak &&
                        a.BinaNo == adres.BinaNo &&
                        a.DaireNo == adres.DaireNo &&
                        a.PostaKodu == adres.PostaKodu
                    );

                    if (ayniAdres != null)
                        mevcutSiparis.AdresID = ayniAdres.AdresID;
                    else
                    {
                        db.Adresler.Add(adres);
                        db.SaveChanges();
                        mevcutSiparis.AdresID = adres.AdresID;
                    }

                    mevcutSiparis.SiparisNo = siparis.SiparisNo;
                    mevcutSiparis.MusteriID = siparis.MusteriID;
                    mevcutSiparis.KullaniciID = currentUserId;

                    // Eski kalemleri stoğa iade et ve iade hareketi oluştur
                    foreach (var eski in eskiKalemler)
                    {
                        var eskiStok = db.DepoStok.FirstOrDefault(s =>
                            s.DepoID == eski.DepoID && s.UrunID == eski.UrunID);

                        if (eskiStok != null)
                            eskiStok.Miktar += eski.Adet;

                        var yeniKalem = urunler.FirstOrDefault(yeni =>
                            yeni.UrunID == eski.UrunID &&
                            yeni.DepoID == eski.DepoID &&
                            yeni.BirimFiyat == eski.BirimFiyat);

                        bool iptalKaynakliMi = true;

                        if (yeniKalem != null)
                        {
                            if (yeniKalem.Adet >= eski.Adet)
                                iptalKaynakliMi = false;
                            else
                                iptalKaynakliMi = true;
                        }

                        db.StokHareketleri.Add(new StokHareketleri
                        {
                            UrunID = eski.UrunID,
                            DepoID = eski.DepoID,
                            KullaniciID = currentUserId,
                            SiparisKalemID = eski.SiparisKalemID,
                            Miktar = eski.Adet,
                            BirimFiyat = eski.BirimFiyat,
                            HareketTipi = 1,
                            HareketTarihi = DateTime.Now,
                            IptalKaynakli = iptalKaynakliMi
                        });
                    }


                    // Eski kalem ve hareketleri sil
                    var stokHareketleri = db.StokHareketleri
                        .Where(sh => sh.SiparisKalemleri.SiparisID == siparis.SiparisID).ToList();

                    db.StokHareketleri.RemoveRange(stokHareketleri);
                    db.SiparisKalemleri.RemoveRange(eskiKalemler);

                    // Yeni kalemleri ekle ve stok düş
                    foreach (var kalem in urunler)
                    {
                        kalem.SiparisID = siparis.SiparisID;
                        db.SiparisKalemleri.Add(kalem);

                        var stok = db.DepoStok.FirstOrDefault(s => s.DepoID == kalem.DepoID && s.UrunID == kalem.UrunID);
                        stok.Miktar -= kalem.Adet;
                    }

                    db.SaveChanges();

                    // Yeni stok hareketlerini oluştur
                    foreach (var kalem in db.SiparisKalemleri.Where(x => x.SiparisID == siparis.SiparisID).ToList())
                    {
                        db.StokHareketleri.Add(new StokHareketleri
                        {
                            UrunID = kalem.UrunID,
                            DepoID = kalem.DepoID,
                            KullaniciID = currentUserId,
                            SiparisKalemID = kalem.SiparisKalemID,
                            Miktar = kalem.Adet,
                            BirimFiyat = kalem.BirimFiyat,
                            HareketTipi = 2,
                            HareketTarihi = DateTime.Now,
                            IptalKaynakli = false
                        });
                    }

                    db.SaveChanges();
                }


                return 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("HATA: " + ex.Message);
                return -1;
            }
        }


        [HttpPost]
        public int Sil(int id)
        {
            try
            {
                var s = db.Siparisler.FirstOrDefault(x => x.SiparisID == id);
                if (s == null) return 1;

                var kalemler = db.SiparisKalemleri.Where(x => x.SiparisID == id).ToList();
                foreach (var k in kalemler)
                    db.SiparisKalemleri.Remove(k);

                var a = db.Adresler.FirstOrDefault(x => x.AdresID == s.AdresID);
                if (a != null)
                    db.Adresler.Remove(a);

                db.Siparisler.Remove(s);
                db.SaveChanges();
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        [HttpGet]
        public JsonResult SiparisGetir(int id)
        {
            var siparis = db.Siparisler
                .Where(x => x.SiparisID == id)
                .Select(x => new
                {
                    x.SiparisID,
                    x.SiparisNo,
                    x.SiparisTarihi,
                    x.SiparisDurumu,
                    x.FaturalandiMi,
                    x.MusteriID,
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
                    },
                    Urunler = x.SiparisKalemleri.Select(k => new
                    {
                        k.UrunID,
                        k.DepoID,
                        k.Adet,
                        k.BirimFiyat
                    }).ToList()
                })
                .FirstOrDefault();

            return Json(siparis, JsonRequestBehavior.AllowGet);
        }



        public JsonResult SiparisDetayGetir(int id)
        {
            var detaylar = db.v_siparisdetay.Where(x => x.SiparisID == id).ToList();
            return Json(detaylar, JsonRequestBehavior.AllowGet);
        }

        private string YeniSiparisNoUret()
        {
            string prefix = "SIP";
            string yil = DateTime.Now.Year.ToString();

            // O yıl içinde kaç sipariş varsa, ona göre sıradaki numarayı verme
            int yilIciSayac = db.Siparisler
                .Count(s => s.SiparisTarihi.Year == DateTime.Now.Year);

            string siparisNo;
            int deneme = 1;

            do
            {
                int siradaki = yilIciSayac + deneme;
                siparisNo = $"{prefix}{yil}{siradaki:D3}"; // Örn: SIP2025002
                deneme++;
            }
            while (db.Siparisler.Any(s => s.SiparisNo == siparisNo)); // Unique

            return siparisNo;
        }

        [HttpPost]
        public int SiparisDurumuGuncelle(int siparisID, string yeniDurum)
        {
            try
            {
                var sid = Session["kullaniciID"]?.ToString();
                int currentUserId;
                if (!int.TryParse(sid, out currentUserId))
                    return -2;

                var siparis = db.Siparisler.FirstOrDefault(s => s.SiparisID == siparisID);
                if (siparis == null) return 1;

                if (siparis.FaturalandiMi && yeniDurum != "Teslim Edildi")
                    return 2;

                if (yeniDurum == "Iptal")
                {
                    var siparisKalemleri = db.SiparisKalemleri.Where(k => k.SiparisID == siparisID).ToList();
                    foreach (var kalem in siparisKalemleri)
                    {
                        var stok = db.DepoStok.FirstOrDefault(s => s.DepoID == kalem.DepoID && s.UrunID == kalem.UrunID);
                        if (stok != null)
                        {
                            stok.Miktar += kalem.Adet;

                            db.StokHareketleri.Add(new StokHareketleri
                            {
                                UrunID = kalem.UrunID,
                                DepoID = kalem.DepoID,
                                KullaniciID = currentUserId,
                                SiparisKalemID = kalem.SiparisKalemID,
                                Miktar = kalem.Adet,
                                BirimFiyat = kalem.BirimFiyat,
                                HareketTipi = 1, // Giriş
                                HareketTarihi = DateTime.Now,
                                IptalKaynakli = true
                            });
                        }
                    }
                }

                siparis.SiparisDurumu = yeniDurum;
                db.SaveChanges();
                return 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("DURUM GÜNCELLEME HATASI: " + ex.Message);
                return -1;
            }
        }
    }
}