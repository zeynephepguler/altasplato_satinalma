using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace altasplato_satinalma.Models
{
    
    public class SatinAlmaSiparis
    {
        [Key]  
        public int SiparisID { get; set; }

        [Required]  // Zorunlu alan
        public int TalepID { get; set; }

        [Required]
        public string UrunAdi { get; set; }
        public string TedarikciAdi { get; set; }

        public int StokID { get; set; }

         
        public string UrunAdedi { get; set; }
        public string BirimFiyat { get; set; }


        [Required]
        public DateTime SiparisTarihi { get; set; }
        public DateTime SiparisTerminTarihi { get; set; }


        [Required]
        [StringLength(50)]
        public string OnayDurumu { get; set; }

        [Required]
        [StringLength(50)]
        public string Durum { get; set; }

        public string Konum { get; set; }

        public string SiparisNotu { get; set; }

       
        public byte[]? DosyaVerisi { get; set; }
    }
}
