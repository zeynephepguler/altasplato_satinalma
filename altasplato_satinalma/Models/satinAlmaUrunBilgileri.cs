using System.ComponentModel.DataAnnotations;

namespace altasplato_satinalma.Models
{
    public class satinAlmaUrunBilgileri
    {
        [Key]
        public int UrunID { get; set; } // Benzersiz ürün kimliği

        [Required]
        [StringLength(200)]
        public string UrunAdi { get; set; } // Ürün adı

        [Required]
        [StringLength(50)]
        public string UrunKodu { get; set; } // Benzersiz ürün kodu

        [StringLength(100)]
        public string Kategori { get; set; } // Ürün kategorisi

        [StringLength(500)]
        public string Aciklama { get; set; } // Ürün açıklaması

        [Range(0, double.MaxValue)]
        public decimal StokMiktari { get; set; } = 0; // Stok miktarı

        [Required]
        [StringLength(50)]
        public string Birim { get; set; } // Ürün birimi (ör: adet, kg)

        [StringLength(150)]
        public string Tedarikci { get; set; } // Tedarikçi adı

        [Range(0, double.MaxValue)]
        public decimal? BirimFiyati { get; set; } // Ürün birim fiyatı (opsiyonel)

        public DateTime? SonKullanmaTarihi { get; set; } // Son kullanma tarihi (opsiyonel)

        public DateTime KayitTarihi { get; set; } = DateTime.Now; // Ürünün kaydedildiği tarih

        public DateTime GuncellemeTarihi { get; set; } = DateTime.Now; // Son güncelleme tarihi
        public int MinStok { get; set; } = 0;
        public string UrunKart { get; set; } = "-";


    }
}
