using MiniERPprojesi.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;

namespace MiniERPprojesi.Controllers
{
    [Authorize(Roles = "Siparis ve Faturalama Sorumlusu")]
    public class SiparisFaturaController : Controller
    {
        MiniERPDBEntities6 db = new MiniERPDBEntities6();

        public ActionResult Index()
        {
            var today = DateTime.Today;

            //  Sipariş durumları
            var gecerliDurumlar = new[] { "Hazirlaniyor", "Teslim Edildi", "Iptal" };

            // Sipariş durumu gruplama
            var durumlar = db.Siparisler
                .Where(s => gecerliDurumlar.Contains(s.SiparisDurumu))
                .GroupBy(s => s.SiparisDurumu)
                .Select(g => new
                {
                    Durum = g.Key,
                    Adet = g.Count()
                }).ToList();

            ViewBag.SiparisDurumAdlari = durumlar.Select(x => x.Durum).ToList();
            ViewBag.SiparisDurumAdetleri = durumlar.Select(x => x.Adet).ToList();

            // Bugünkü sipariş ve fatura sayısı
            ViewBag.BugunkuSiparis = db.Siparisler.Count(s => s.SiparisTarihi >= today);
            ViewBag.BugunkuFatura = db.Faturalar.Count(f => f.FaturaTarihi >= today);

            // Faturalama bilgisi
            int toplam = db.Siparisler.Count();
            int faturalanan = db.Siparisler.Count(s => s.FaturalandiMi == true);
            ViewBag.Faturalanan = faturalanan;
            ViewBag.Faturalanmayan = toplam - faturalanan;

            // Son 5 geçerli sipariş (geçersizler hariç)
            ViewBag.SonSiparisler = db.v_siparisler
                .Where(s => gecerliDurumlar.Contains(s.SiparisDurumu))
                .OrderByDescending(x => x.SiparisTarihi)
                .Take(5)
                .ToList();

            // Son 5 fatura

            var sonFaturalar = db.v_faturalar
    .OrderByDescending(x => x.FaturaTarihi)
    .Take(5)
    .ToList();

            ViewBag.SonFaturalar = sonFaturalar;



            return View();
        }

    }
}
