using System;
using System.ComponentModel.DataAnnotations;
namespace altasplato_satinalma.Models
{
    public class Degerlendir
    {
        public int Id { get; set; }

        public string DokumanNo { get; set; }

        
        public string RevizyonNo { get; set; }

        
        public DateTime? RevizyonTarihi { get; set; }

       
        public DateTime? DegerlendirmeTarihi { get; set; }

       
        public string Adi { get; set; }

        public string TelefonNo { get; set; }

        public string EPosta { get; set; }

        public string IlgiliKisi { get; set; }

        public string FaaliyetKonusu { get; set; }

        public int Fiyat { get; set; }

        public int OdemeEsnekligi { get; set; }

        public int UrunKalitesi { get; set; }

        public int Zamanindalik { get; set; }

        public int Uzmanlik { get; set; }

        public int DestekDokumanlar { get; set; }

        public int SatisSonrasiHizmet { get; set; }

        public int Sureklilik { get; set; }

        public int KaliteYonetimSistemi { get; set; }

        public string DofAcilsinMi { get; set; }

        public string DofAciklama { get; set; }

        [Required]
        public string Siniflandirma { get; set; }

        public int? DegerlendirmeSonucu { get; set; }

        public string DegerlendirenAdSoyad { get; set; }
    }
}
