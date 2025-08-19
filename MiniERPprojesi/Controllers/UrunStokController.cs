using MiniERPprojesi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MiniERPprojesi.Controllers
{
    [Authorize(Roles = "Ürün ve Stok Sorumlusu")]
    public class UrunStokController : Controller
    {
        MiniERPDBEntities6 db = new MiniERPDBEntities6();
        public ActionResult Index()
        {
            // Genel sayılar
            ViewBag.UrunSayisi = db.Urunler.Count();
            ViewBag.DepoSayisi = db.Depolar.Count();
            ViewBag.KritikSayisi = db.DepoStok.Count(x => x.Miktar < 10); // kritik stok limiti

            // Bugünkü çıkış işlemleri (HareketTipi = 2 - Çıkış)
            var bugun = DateTime.Today;
            ViewBag.BugunkuCikis = db.StokHareketleri
                .Count(x => x.HareketTarihi >= bugun && x.HareketTipi == 2);

            // Grafik: Ürün bazlı depo stokları (Grouped Bar Chart için)
            var depolar = db.Depolar.OrderBy(d => d.DepoID).ToList();
            var urunler = db.Urunler.OrderBy(u => u.UrunID).ToList();

            ViewBag.DepoAdlari = depolar.Select(d => d.DepoAdi).ToList();
            ViewBag.UrunAdlari = urunler.Select(u => u.UrunAdi).ToList();

            var stokVerileri = new List<List<int>>();

            foreach (var urun in urunler)
            {
                var urunStokListesi = new List<int>();

                foreach (var depo in depolar)
                {
                    int miktar = db.DepoStok
                        .Where(s => s.UrunID == urun.UrunID && s.DepoID == depo.DepoID)
                        .Select(s => (int?)s.Miktar)
                        .FirstOrDefault() ?? 0;

                    urunStokListesi.Add(miktar);
                }

                stokVerileri.Add(urunStokListesi);
            }

            ViewBag.StokVerileri = stokVerileri;

            // Kritik stokta olan ürünler
            ViewBag.KritikUrunler = db.v_depostok
                .Where(x => x.Miktar < 10)
                .OrderBy(x => x.Miktar)
                .ToList();

            // Son 5 stok hareketi
            ViewBag.SonHareketler = db.v_stokhareketleri
                .OrderByDescending(x => x.HareketTarihi)
                .Take(5)
                .ToList();

            return View();
        }
    }
}
