using System.Web.Mvc;

namespace WebUygulama.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            string rol = Session["rol"]?.ToString();

            switch (rol)
            {
                case "Admin":
                    return RedirectToAction("Index", "Admin");
                case "Ürün ve Stok Sorumlusu":
                    return RedirectToAction("Index", "UrunStok");
                case "Siparis ve Faturalama Sorumlusu":
                    return RedirectToAction("Index", "SiparisFatura");
                default:
                    return View();
            }
        }
    }
}
