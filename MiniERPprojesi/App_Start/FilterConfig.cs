using System.Web.Mvc;

namespace MiniERPprojesi
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            // Hata sayfası için varsayılan filtre
            filters.Add(new HandleErrorAttribute());

            // 🔒 Oturum açılmadan hiçbir sayfaya erişilemesin
            filters.Add(new AuthorizeAttribute());
        }
    }
}
