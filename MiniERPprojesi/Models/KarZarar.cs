namespace MiniERPprojesi.Models
{
    public class KarZarar
    {
        public string Ay { get; set; }
        public decimal SatisGeliri { get; set; }
        public decimal Maliyet { get; set; }
        public decimal Kar => SatisGeliri - Maliyet;
    }
}
