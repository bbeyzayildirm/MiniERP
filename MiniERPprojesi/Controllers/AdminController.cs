using MiniERPprojesi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MiniERPprojesi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        MiniERPDBEntities6 db = new MiniERPDBEntities6();

        public ActionResult Index()
        {
            // Sayısal özet bilgileri
            ViewBag.KullaniciSayisi = db.Kullanicilar.Count();
            ViewBag.UrunSayisi = db.Urunler.Count();
            ViewBag.DepoSayisi = db.Depolar.Count();
            ViewBag.SiparisSayisi = db.Siparisler.Count();
            ViewBag.FaturaSayisi = db.Faturalar.Count();

            // Son 5 sipariş
            var son5 = db.v_siparisler
                .OrderByDescending(s => s.SiparisTarihi)
                .Take(5)
                .ToList();

            // Kar-Zarar
            var karZarar = KarZararHesapla();

            return View(Tuple.Create(son5, karZarar));
        }

        // Kar-Zarar verisini hesaplayan ortak fonksiyon
        private List<KarZarar> KarZararHesapla()
        {
            return db.StokHareketleri
                .Where(s => !s.IptalKaynakli && s.HareketTarihi != null)
                .AsEnumerable()
                .GroupBy(s => s.HareketTarihi.ToString("yyyy-MM"))
                .Select(g => new KarZarar
                {
                    Ay = g.Key,
                    SatisGeliri = g.Where(x => x.HareketTipi == 2).Sum(x => x.Miktar * x.BirimFiyat),
                    Maliyet = g.Where(x => x.HareketTipi == 1).Sum(x => x.Miktar * x.BirimFiyat)
                })
                .OrderBy(x => x.Ay)
                .ToList();
        }

        // Kar-Zarar grafiği için JSON veri
        public JsonResult KarZararVerisiGetir()
        {
            var karZarar = KarZararHesapla();

            var jsonData = karZarar.Select(x => new
            {
                Ay = x.Ay,
                Alis = x.Maliyet,
                Satis = x.SatisGeliri,
                KarZarar = x.Kar
            }).ToList();

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }
    }
}
