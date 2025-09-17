using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace altasplato_satinalma.Models
{
    [Table("satinAlmaDepos")]
    public class SatinAlmaDepos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int TedarikciId { get; set; }

        [Required]
        [MaxLength(255)]
        public string UrunAdi { get; set; }

        [Required]
        public int KalanAdet { get; set; }

        [Required]
        [MaxLength(255)]
        public string Kategori { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal BirimFiyat { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime SiparisOnayTarihi { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime SiparisTerminTarihi { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime UrunGelisTarihi { get; set; }

        [Required]
        public int GelenUrunMiktari { get; set; }

        [MaxLength(255)]
        public string KontrolEden { get; set; }

        [MaxLength(255)]
        public string Karar { get; set; }
        public string Aciklama { get; set; } = "-";

        public string FotoPath { get; set; } = "null"; // Fotoğraf yolu

        [Required]
        [DataType(DataType.Date)]
        public DateTime EklemeTarihi { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? GuncellemeTarihi { get; set; }
    }
}
