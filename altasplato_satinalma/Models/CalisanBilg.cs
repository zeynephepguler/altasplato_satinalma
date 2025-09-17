using System.ComponentModel.DataAnnotations;

namespace altasplato_satinalma.Models
{
    public class CalisanBilg
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserLastname { get; set; }
        public string? AmirIsmi { get; set; }
        public DateTime? UserBirthday { get; set; }

        public string? UserShort { get; set; }
        public string? Bolum { get; set; }
    }
}
