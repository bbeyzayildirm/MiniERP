using System;
using System.Configuration;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MiniERPprojesi.Controllers
{
    [Authorize]
    public class WeatherController : Controller
    {
        private static readonly HttpClient http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(8)
        };

        [HttpGet]
        public async Task<ActionResult> GetWeather(string sehir = "Istanbul")
        {
            // Config'ten anahtarları oku
            var apiKey = ConfigurationManager.AppSettings["OWM_ApiKey"];
            var host = (ConfigurationManager.AppSettings["OWM_ApiHost"] ?? "https://api.openweathermap.org").TrimEnd('/');

            if (string.IsNullOrWhiteSpace(apiKey))
                return Json(new { ok = false, error = "API anahtarı tanımlı değil." }, JsonRequestBehavior.AllowGet);

            if (string.IsNullOrWhiteSpace(sehir)) sehir = "Istanbul";
            var safeCity = Uri.EscapeDataString(sehir);

            var url = $"{host}/data/2.5/weather?q={safeCity}&appid={apiKey}&units=metric&lang=tr";

            try
            {
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                using (var resp = await http.GetAsync(url, cts.Token))
                {
                    if (!resp.IsSuccessStatusCode)
                        return Json(new { ok = false, status = (int)resp.StatusCode }, JsonRequestBehavior.AllowGet);

                    var json = await resp.Content.ReadAsStringAsync();
                    var raw = JsonConvert.DeserializeObject<WeatherResult>(json);

                    if (raw == null || raw.main == null)
                        return Json(new { ok = false, error = "Geçersiz veri." }, JsonRequestBehavior.AllowGet);

                    return Json(new
                    {
                        name = raw.name,
                        main = raw.main,
                        weather = raw.weather
                    }, JsonRequestBehavior.AllowGet);

                }
            }
            catch (TaskCanceledException)
            {
                return Json(new { ok = false, error = "Zaman aşımı." }, JsonRequestBehavior.AllowGet);
            }
            catch (HttpRequestException)
            {
                return Json(new { ok = false, error = "Ağ/HTTP hatası." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { ok = false, error = "Beklenmeyen hata." }, JsonRequestBehavior.AllowGet);
            }
        }
    }

    // API modelin
    public class WeatherResult
    {
        public Main main { get; set; }
        public List<Weather> weather { get; set; }
        public string name { get; set; }
    }

    public class Main { public double temp { get; set; } }
    public class Weather { public string description { get; set; } public string icon { get; set; } }
}
