using System.ComponentModel.DataAnnotations;

namespace altasplato_satinalma.Models
{
    public class satinAlmaTalep
    {
        [Key]
        public int TalepNo { get; set; } 

        [Required]
        [DataType(DataType.Date)]
        public DateTime TalepTarihi { get; set; }

        [Required]
        [StringLength(100)]
        public string UrunKodu { get; set; }

        [Required]
        [StringLength(100)]
        public string TalepEdenBolum { get; set; } 

        [Required]
        [StringLength(200)]
        public string TalepEdilenUrun { get; set; } 

        [Required]
        public int TalepEdilenMiktar { get; set; } 

        [Required]
        [StringLength(10)]
        public string Birim { get; set; } 
    }
}
