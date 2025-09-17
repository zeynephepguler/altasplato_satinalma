using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace altasplato_satinalma.Models
{
    public class satinAlmaSOnay
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SiparisID { get; set; } // Benzersiz Sipariş ID

        [Required]
        public int TalepID { get; set; } // Talep Tablosuyla İlişkili ID

        [Required]
        public int StokID { get; set; } // Stok Tablosuyla İlişkili ID

        [Required]
        [DataType(DataType.Date)]
        public DateTime SiparisTarihi { get; set; } // Sipariş Tarihi

        [Required]
        [MaxLength(50)]
        public string OnayDurumu { get; set; } // Sipariş Onay Durumu

        [Required]
        [MaxLength(50)]
        public string Durum { get; set; } // Ürünün Durumu

        [MaxLength(255)]
        public string SiparisNotu { get; set; } // Ek Notlar

       

    }
}
