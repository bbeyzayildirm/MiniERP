using MiniERPprojesi.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MiniERPprojesi.Helpers;

namespace MiniERPprojesi.Controllers
{
    public class GirisController : Controller
    {
        MiniERPDBEntities6 db = new MiniERPDBEntities6();

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(string kullaniciAdi, string sifre)
        {
            // Şifreyi hashle
            string hashliSifre = SifreHelper.SifreHashle(sifre);

            // Hash'lenmiş şifreyle kullanıcıyı kontrol et
            var kullanici = db.Kullanicilar
                .FirstOrDefault(x => x.KullaniciAdi == kullaniciAdi && x.SifreHash == hashliSifre);

            if (kullanici != null)
            {
                // Cookie oluştur
                FormsAuthentication.SetAuthCookie(kullanici.KullaniciAdi, false);

                Session["kullaniciID"] = kullanici.KullaniciID;
                Session["kullaniciAd"] = kullanici.Ad + " " + kullanici.Soyad;
                Session["rol"] = kullanici.Roller.RolAdi;
                Session["profilFotoUrl"] = kullanici.ProfilFotoUrl ?? "/Content/dist/img/user2-160x160.jpg";

                // Kullanıcı giriş yaptığında KullaniciGirisCikisLog tablosuna yeni bir satır ekleme
                var log = new KullaniciGirisCikisLog
                {
                    KullaniciID = kullanici.KullaniciID,
                    GirisZamani = DateTime.Now
                };
                db.KullaniciGirisCikisLog.Add(log);
                db.SaveChanges();

                // Bu log kaydının ID’si session’da tutulur. Çıkışta hangi kaydın güncelleneceğini bilmek için.
                Session["GirisLogID"] = log.ID;

                // Rol yönlendirme
                switch (kullanici.Roller.RolAdi)
                {
                    case "Admin": return RedirectToAction("Index", "Admin");
                    case "Ürün ve Stok Sorumlusu": return RedirectToAction("Index", "UrunStok");
                    case "Siparis ve Faturalama Sorumlusu": return RedirectToAction("Index", "SiparisFatura");
                    default: return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Hata = "Kullanıcı adı veya şifre hatalı.";
            return View();
        }

        [Authorize]
        public ActionResult Cikis()
        {
            // Oturum logunu güncelle (çıkış zamanı)
            int? logId = Session["GirisLogID"] as int?;
            // Kullanıcı giriş yaptığında oluşturulan log kaydının ID’sini alır.
            if (logId != null)
            {
                var log = db.KullaniciGirisCikisLog.Find(logId.Value);

                if (log != null)
                {
                    log.CikisZamani = DateTime.Now;
                    db.SaveChanges();
                }
            }

            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Login", "Giris");
        }

    }
}
