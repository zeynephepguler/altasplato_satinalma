using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace altasplato_satinalma.Models
{
    [Table("satinAlmaTedarikcilers")]
    public class SatinAlmaTedarikciler
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string TedarikciAdi { get; set; }

        [MaxLength(50)]
        public string TedarikciTelefon { get; set; }

        public string TedarikciAdres { get; set; }
        public string TedarikciMail { get; set; }
        public string TedarikciNot { get; set; } = "-";

        public int Score { get; set; } = 0;

    }
}
