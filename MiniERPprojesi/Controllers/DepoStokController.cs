using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MiniERPprojesi.Models;

namespace MiniERPprojesi.Controllers
{
    [Authorize(Roles = "Admin,Ürün ve Stok Sorumlusu")]
    public class DepoStokController : Controller
    {
        MiniERPDBEntities6 db = new MiniERPDBEntities6();

        public ActionResult Index(int? depoID, int? urunID)
        {
            ViewBag.Depolar = db.Depolar.ToList();
            ViewBag.Urunler = db.Urunler.ToList();

            var stoklar = db.DepoStok.AsQueryable();

            if (depoID.HasValue)
                stoklar = stoklar.Where(s => s.DepoID == depoID.Value);

            if (urunID.HasValue)
                stoklar = stoklar.Where(s => s.UrunID == urunID.Value);

            ViewBag.SeciliDepo = depoID;
            ViewBag.SeciliUrun = urunID;

            return View(stoklar.ToList());
        }


        // AJAX: Ürün girişi
        [HttpPost]
        public int UrunGir(int depoID, int urunID, int miktar, decimal birimFiyat)
        {
            var sid = Session["kullaniciID"]?.ToString();
            int kullaniciId;
            if (!int.TryParse(sid, out kullaniciId))
                return -2; // Oturum yok

            try
            {
                // Güncelle veya ekle
                var stok = db.DepoStok.FirstOrDefault(x => x.DepoID == depoID && x.UrunID == urunID);
                if (stok == null)
                {
                    db.DepoStok.Add(new DepoStok
                    {
                        DepoID = depoID,
                        UrunID = urunID,
                        Miktar = miktar
                    });
                }
                else
                {
                    stok.Miktar += miktar;
                }

                // Stok hareketini kaydet (Giriş)
                db.StokHareketleri.Add(new StokHareketleri
                {
                    DepoID = depoID,
                    UrunID = urunID,
                    Miktar = miktar,
                    BirimFiyat = birimFiyat,
                    HareketTipi = 1, // Giriş
                    HareketTarihi = DateTime.Now,
                    KullaniciID = kullaniciId,
                    IptalKaynakli = false
                });

                db.SaveChanges();
                return 0;
            }
            catch
            {
                return -1;
            }
        }

    }
}
