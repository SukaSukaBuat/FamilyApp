using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyApp.Common.Dtos
{
    public class SignUpDto
    {
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = null!;
        [StringLength(maximumLength: 20, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = null!;
        [StringLength(200)]
        public string? Remarks { get; set; }
        public string UserName => Email;
    }
}
