using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LoginPopup.Models
{
    public class LoginModel
    {

        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string EmailAddress { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{6,}$", ErrorMessage = "Password must be at least 6 characters long and contain at least one letter and one number.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }
}
