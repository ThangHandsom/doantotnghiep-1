using System.ComponentModel.DataAnnotations;

namespace QLTours.Models
{
    public class ResetPasswordViewModel
    {
        public string ResetCode { get; set; } 
        public string Email { get; set; } 
        public string NewPassword { get; set; } 
    }
}
