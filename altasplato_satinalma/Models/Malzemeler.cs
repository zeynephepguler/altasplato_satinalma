namespace altasplato_satinalma.Models
{
    public class Malzemeler
    {
        public int Id { get; set; }
        public int FormId { get; set; } 
        public string MalzemeAdi { get; set; } = "-";
        public string MevcutStok { get; set; } = "-";
        public string TalepMiktari { get; set; } = "-";
        public DateTime? TerminTarihi { get; set; }
        public string Aciklama { get; set; } = "-";
        public string Durum { get; set; } = "-";
        public bool Urun { get; set; } = false;

        public string TalepEden { get; set; } = "-";

        public Formlar Form { get; set; }

    }
}
