namespace altasplato_satinalma.Models
{
    public class Bildirimler
    {
        public int Id { get; set; }
        public int KartNo { get; set; }

        public string Mesaj { get; set; }
        public DateTime OlusturmaTarihi { get; set; }
        public bool Okundu { get; set; }  // Bildirimin okunup okunmadığını tutar
    }
}
