using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MiniERPprojesi.Models;
using MiniERPprojesi.Helpers;

namespace MiniERPprojesi.Controllers
{
    [Authorize]
    public class KullaniciController : Controller
    {
        MiniERPDBEntities6 db = new MiniERPDBEntities6();

        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var kullanicilar = db.Kullanicilar.ToList();

            ViewBag.Roller = db.Roller
                .Select(r => new SelectListItem
                {
                    Value = r.RolID.ToString(),
                    Text = r.RolAdi
                }).ToList();

            return View(kullanicilar);
        }

        // Yeni Kayıt / Güncelleme - Fotoğraflı (FormData)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public JsonResult EkleForm()
        {
            try
            {
                string idStr = Request.Form["KullaniciID"];
                int id = 0;
                int.TryParse(idStr, out id);

                string kullaniciAdi = Request.Form["KullaniciAdi"];
                string sifre = Request.Form["SifreHash"];
                string email = Request.Form["Email"];
                string ad = Request.Form["Ad"];
                string soyad = Request.Form["Soyad"];
                string rolStr = Request.Form["RolID"];

                // Zorunlu alan kontrolü
                if (string.IsNullOrWhiteSpace(kullaniciAdi) ||
                    string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(ad) ||
                    string.IsNullOrWhiteSpace(soyad) ||
                    string.IsNullOrWhiteSpace(rolStr))
                {
                    return Json(new { success = false, message = "Lütfen tüm zorunlu alanları doldurunuz." });
                }

                if (!int.TryParse(rolStr, out int rolId) || rolId <= 0)
                    return Json(new { success = false, message = "Geçersiz rol seçimi yapıldı." });

                // Yeni kullanıcı için şifre zorunlu
                if (id == 0 && string.IsNullOrWhiteSpace(sifre))
                    return Json(new { success = false, message = "Yeni kullanıcı için şifre girilmelidir." });

                // Şifre hash
                if (!string.IsNullOrWhiteSpace(sifre))
                    sifre = SifreHelper.SifreHashle(sifre);

                // Profil fotoğrafı kontrolü
                string fotoYolu = null;
                if (Request.Files.Count > 0)
                {
                    var file = Request.Files[0];
                    if (file != null && file.ContentLength > 0)
                    {
                        var ext = Path.GetExtension(file.FileName).ToLower();
                        var izinli = new[] { ".jpg", ".jpeg", ".png" };
                        if (!izinli.Contains(ext))
                        {
                            return Json(new { success = false, message = "Sadece .jpg, .jpeg veya .png uzantılı fotoğraflar yüklenebilir." });
                        }

                        var yeniAd = Guid.NewGuid().ToString() + ext;
                        var yol = "/Uploads/ProfilFotograflari/" + yeniAd;
                        var klasor = Server.MapPath("/Uploads/ProfilFotograflari");
                        Directory.CreateDirectory(klasor);
                        file.SaveAs(Path.Combine(klasor, yeniAd));
                        fotoYolu = yol;
                    }
                }

                // Yeni kayıt mı güncelleme mi
                Kullanicilar k;
                if (id == 0)
                {
                    k = new Kullanicilar();
                    db.Kullanicilar.Add(k);
                }
                else
                {
                    k = db.Kullanicilar.FirstOrDefault(x => x.KullaniciID == id);
                    if (k == null)
                        return Json(new { success = false, message = "Güncellenecek kullanıcı bulunamadı." });
                }

                // Değer atamaları
                k.KullaniciAdi = kullaniciAdi;
                if (!string.IsNullOrWhiteSpace(sifre))
                    k.SifreHash = sifre;
                k.Email = email;
                k.Ad = ad;
                k.Soyad = soyad;
                k.RolID = rolId;
                if (!string.IsNullOrEmpty(fotoYolu))
                    k.ProfilFotoUrl = fotoYolu;

                db.SaveChanges();

                // Eğer giriş yapan kişi güncelleniyorsa Session güncelle
                int? oturumKullaniciId = Session["kullaniciID"] as int?;
                if (oturumKullaniciId != null && oturumKullaniciId == k.KullaniciID)
                {
                    Session["kullaniciAd"] = k.Ad + " " + k.Soyad;
                    Session["email"] = k.Email;
                    Session["profilFotoUrl"] = k.ProfilFotoUrl;
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Günlükleme
                System.Diagnostics.Debug.WriteLine("🚨 HATA: " + ex.Message);
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine("INNER: " + ex.InnerException.Message);
                }

                return Json(new { success = false, message = "Beklenmeyen bir hata oluştu. Lütfen tekrar deneyiniz." });
            }
        }


        // Artık sadece form datasız kullanıyorsan bu kullanılmaz
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public int Ekle(Kullanicilar k)
        {
            try
            {
                if (k.KullaniciID == 0)
                {
                    db.Kullanicilar.Add(k);
                }
                else
                {
                    var mevcut = db.Kullanicilar.FirstOrDefault(x => x.KullaniciID == k.KullaniciID);
                    if (mevcut == null) return 1;

                    mevcut.KullaniciAdi = k.KullaniciAdi;

                    // Burada da istenirse güvenli yapılabilir
                    if (!string.IsNullOrWhiteSpace(k.SifreHash))
                        mevcut.SifreHash = k.SifreHash;

                    mevcut.Email = k.Email;
                    mevcut.Ad = k.Ad;
                    mevcut.Soyad = k.Soyad;
                    mevcut.RolID = k.RolID;
                }

                db.SaveChanges();
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public int Sil(int id)
        {
            try
            {
                var k = db.Kullanicilar.Find(id);
                if (k == null) return 1;

                db.Kullanicilar.Remove(k);
                db.SaveChanges();
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public JsonResult Getir(int id)
        {
            var k = db.Kullanicilar
                .Where(x => x.KullaniciID == id)
                .Select(x => new
                {
                    x.KullaniciID,
                    x.KullaniciAdi,
                    x.SifreHash,
                    x.Email,
                    x.Ad,
                    x.Soyad,
                    x.RolID,
                    x.ProfilFotoUrl
                })
                .FirstOrDefault();

            return Json(k, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Profil()
        {
            int? kullaniciId = Session["kullaniciID"] as int?;
            if (kullaniciId == null)
                return RedirectToAction("Index", "Home");

            var kullanici = db.Kullanicilar.FirstOrDefault(x => x.KullaniciID == kullaniciId);
            if (kullanici == null)
                return HttpNotFound();

            return View(kullanici);
        }

        [HttpGet]
        public ActionResult ProfilGuncelle()
        {
            int? kullaniciId = Session["kullaniciID"] as int?;
            if (kullaniciId == null)
                return RedirectToAction("Login", "Giris");

            var kullanici = db.Kullanicilar.FirstOrDefault(x => x.KullaniciID == kullaniciId);
            if (kullanici == null)
                return HttpNotFound();

            return View(kullanici);
        }

        [HttpPost]
        public ActionResult ProfilGuncelle(Kullanicilar model, HttpPostedFileBase profilFoto)
        {
            int? kullaniciId = Session["kullaniciID"] as int?;
            if (kullaniciId == null)
                return RedirectToAction("Login", "Giris");

            var kullanici = db.Kullanicilar.FirstOrDefault(x => x.KullaniciID == kullaniciId);
            if (kullanici == null)
                return HttpNotFound();

            if (!string.IsNullOrWhiteSpace(model.SifreHash))
                kullanici.SifreHash = SifreHelper.SifreHashle(model.SifreHash);


            if (!string.IsNullOrWhiteSpace(model.Ad))
                kullanici.Ad = model.Ad;

            if (!string.IsNullOrWhiteSpace(model.Soyad))
                kullanici.Soyad = model.Soyad;

            if (!string.IsNullOrWhiteSpace(model.Email))
                kullanici.Email = model.Email;

            if (profilFoto != null && profilFoto.ContentLength > 0)
            {
                var ext = Path.GetExtension(profilFoto.FileName).ToLower();
                var izinli = new[] { ".jpg", ".jpeg", ".png" };
                if (izinli.Contains(ext))
                {
                    var yeniAd = Guid.NewGuid().ToString() + ext;
                    var yol = "/Uploads/ProfilFotograflari/" + yeniAd;
                    var tamYol = Server.MapPath(yol);
                    profilFoto.SaveAs(tamYol);
                    kullanici.ProfilFotoUrl = yol;
                }
            }

            db.SaveChanges();

            // Session bilgilerini güncelle
            Session["kullaniciAd"] = kullanici.Ad + " " + kullanici.Soyad;
            Session["email"] = kullanici.Email;
            Session["profilFotoUrl"] = kullanici.ProfilFotoUrl;

            return RedirectToAction("Profil");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult SistemKullanimRaporu()

        {
            var logs = db.KullaniciGirisCikisLog.ToList();
            var kullanicilar = db.Kullanicilar.ToList();
            var liste = new List<KullaniciGirisLogViewModel>();

            foreach (var log in logs)
            {
                var k = kullanicilar.FirstOrDefault(x => x.KullaniciID == log.KullaniciID);
                if (k == null) continue;

                DateTime giris = log.GirisZamani;
                DateTime cikis = log.CikisZamani ?? log.SonPing ?? DateTime.Now;
                int dakika = (int)(cikis - giris).TotalMinutes;

                liste.Add(new KullaniciGirisLogViewModel
                {
                    AdSoyad = (k.Ad + " " + k.Soyad).Trim(),
                    Rol = k.Roller?.RolAdi,
                    GirisZamani = giris,
                    CikisZamani = log.CikisZamani,
                    Dakika = dakika
                });
            }

            return View(liste.OrderByDescending(x => x.GirisZamani).ToList());
        }

        public ActionResult Iletisim()
        {
            var kullanicilar = db.v_kullanicilar.ToList();
            return View(kullanicilar);
        }


        [HttpGet]
        [AllowAnonymous]
        public ActionResult SifremiUnuttum()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult SifremiUnuttum(string email)
        {
            var kullanici = db.Kullanicilar.FirstOrDefault(x => x.Email == email);

            if (kullanici == null)
            {
                ViewBag.Mesaj = "Bu e-posta ile kayıtlı kullanıcı bulunamadı.";
                return View();
            }

            string yeniSifre = Guid.NewGuid().ToString("N").Substring(0, 8);
            kullanici.SifreHash = SifreHelper.SifreHashle(yeniSifre);
            db.SaveChanges();

            try
            {
                string konu = "UYUM ERP - Şifre Sıfırlama";
                string icerik = $"Merhaba {kullanici.Ad} {kullanici.Soyad},\n\nYeni şifreniz: {yeniSifre}\n\nLütfen giriş yaptıktan sonra değiştirin.";
                MailHelper.Send(email, konu, icerik);

                ViewBag.Mesaj = "Yeni şifreniz e-posta adresinize gönderildi.";
            }
            catch (Exception ex)
            {
                ViewBag.Mesaj = "Mail gönderilemedi: " + ex.Message;
            }

            return View();
        }


    }
}
