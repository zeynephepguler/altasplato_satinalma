using System.ComponentModel.DataAnnotations;

namespace altasplato_satinalma.Models
{
    public class Formlar
    {
        [Key]
        public int FormNo { get; set; }
        public string SirketAdi { get; set; } 
        public string FormTuru { get; set; } 
        public string TalepEden { get; set; } 
        public string Onaylayan { get; set; } 
        public DateTime Tarih { get; set; } 
        public string Durum { get; set; }

        public ICollection<Malzemeler> Malzemeler { get; set; } = new List<Malzemeler>();
    }

}
