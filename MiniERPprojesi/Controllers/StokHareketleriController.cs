using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MiniERPprojesi.Models;

namespace MiniERPprojesi.Controllers
{
    [Authorize(Roles = "Admin,Ürün ve Stok Sorumlusu")]
    public class StokHareketleriController : Controller
    {
        MiniERPDBEntities6 db = new MiniERPDBEntities6();

        // Tüm stok hareketlerini v_stokhareketleri üzerinden getir
        public ActionResult Index(int? hareketTipi, int? depoId)
        {
            var hareketler = db.v_stokhareketleri.AsQueryable();

            if (hareketTipi.HasValue)
            {
                hareketler = hareketler.Where(x => x.HareketTipi == hareketTipi.Value);
            }

            if (depoId.HasValue)
            {
                hareketler = hareketler.Where(x => x.DepoID == depoId.Value);
            }

            ViewBag.Depolar = db.Depolar
                .Select(d => new SelectListItem
                {
                    Value = d.DepoID.ToString(),
                    Text = d.DepoAdi
                }).ToList();

            ViewBag.SeciliTip = hareketTipi;
            ViewBag.SeciliDepo = depoId;

            return View(hareketler.OrderByDescending(x => x.HareketTarihi).ToList());
        }


    }
}
