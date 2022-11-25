using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoginPro.Entities
{
    [Table("users")]//veritabanındaki tablo adına da müdahale edilebilir
    public class User
    {
        //6.0 la birlikte property'ler default olarak nullable tanımlanmıyor. Bu yüzden nullable yapmak için yanlarına soru işareti konmak zorunda
        [Key]
        public Guid Id { get; set; }
        [StringLength(50)]
        //default'u nvarchar 100
        public string? FullName { get; set; }
        [Required]
        [StringLength(30)]
        public string Username { get; set; }
        [Required]
        [StringLength(100)]
        public string Password { get; set; }
        public bool Locked { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
