using altasplato_satinalma.Models;

namespace altasplato_satinalma.Models
{
    public class SatinAlmaFaturaView
    {
        public IEnumerable<satinAlmaFatura> deger1 { get; set; } = new List<satinAlmaFatura>();
        public IEnumerable<satinAlmaFaturaKalem> deger2 { get; set; } = new List<satinAlmaFaturaKalem>();
    }
}
