using System;

namespace MiniERPprojesi.Models
{
    public class KullaniciGirisLogViewModel
    {
        public string AdSoyad { get; set; }
        public string Rol { get; set; } // ✅ Yeni alan
        public DateTime GirisZamani { get; set; }
        public DateTime? CikisZamani { get; set; }
        public int Dakika { get; set; }
    }

}