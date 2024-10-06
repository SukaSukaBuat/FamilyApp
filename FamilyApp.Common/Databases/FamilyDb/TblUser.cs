using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyApp.Common.Databases.FamilyDb
{
    public class TblUser : IdentityUser<Guid>
    {
        public bool UserConfirmed { get; set; }
        public override string Email { get; set; } = null!;
        public string ? Remarks { get; set; }
        public OuthProvider OuthProvider { get; set; }
        public virtual ICollection<TblLoginSession> TblLoginSessions { get; set; } = [];
    }
}
