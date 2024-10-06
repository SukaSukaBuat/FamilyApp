using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyApp.Common.Databases.FamilyDb
{
    public class TblAuditTrail
    {
        public Guid Id { get; set; }
        public Guid ActorId { get; set; }
        public EntityState Action { get; set; }
        public string TableName { get; set; } = null!;
        public Guid TableId { get; set; }
        public DateTimeOffset ActionTimestamp { get; set; }
        public string Data { get; set; } = null!;
    }
}
