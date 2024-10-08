using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyApp.Common.Dtos
{
    public class ResetPasswordDto
    {
        [EmailAddress]
        public string Email { get; set; } = null!;
        [StringLength(maximumLength: 20, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = null!;
        public string Token { get; set; } = null!;
    }
}
