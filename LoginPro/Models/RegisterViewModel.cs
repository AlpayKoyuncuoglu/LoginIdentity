using System.ComponentModel.DataAnnotations;

namespace LoginPro.Models
{
    public class RegisterViewModel
    {
        [Display(Name ="Kullanıcı Adı",Prompt="johndoe")]//eğer label tagı arasına bir değer girilmezse bu geçerli olur
        //promt placeholder yerine geçer
        [Required(ErrorMessage = "Username is required")]
        [StringLength(30, ErrorMessage = "Username can be max 30 charachters")]
        public string Username { get; set; }

        //[DataType(DataType.Password)] input iççerisinde type="password de yazılabilir, ya da buradan da yönetilebilir
        [Required(ErrorMessage = "Password is required")]
        [MinLength(6,ErrorMessage ="Password can be min 6 charachters")]
        [MaxLength(16,ErrorMessage ="Password can be max 16 charachters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Re-Password is required")]
        [MinLength(6, ErrorMessage = "Re-Password can be min 6 charachters")]
        [MaxLength(16, ErrorMessage = "Re-Password can be max 16 charachters")]
        //[Compare("Password")]        
        [Compare(nameof(Password))]        
        public string RePassword { get; set; }

    }
}
