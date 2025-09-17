namespace altasplato_satinalma.Models
{
    public class SiparisTalepPdf
    {
        public int Id { get; set; }
        public string DosyaAdi { get; set; }  
        public DateTime KayitTarihi { get; set; } = DateTime.Now;
        public byte[] DosyaVerisi { get; set; }
    }
}
