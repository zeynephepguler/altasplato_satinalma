using System.ComponentModel.DataAnnotations;

namespace altasplato_satinalma.Models
{
    public class SatinAlmaDepoUrun_Es
    {
        [Key]
        public int Id { get; set; }

        public string DepoId { get; set; }
        public string UrunId { get; set; }
        public string UrunNormalizedId { get; set; }
    }
}
