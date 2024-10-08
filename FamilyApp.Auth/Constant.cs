using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyApp.Auth
{
    public static class JwtTokenClaim
    {
        public const string UserId = "user_id";
        public const string Email = "email";
        public const string Role = "role";
        public const string OuthProvider = "outh_provider";
        public const string SessionId = "session_id";
    }
}
