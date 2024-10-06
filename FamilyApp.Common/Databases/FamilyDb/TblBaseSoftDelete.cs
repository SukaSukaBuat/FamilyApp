using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyApp.Common.Databases.FamilyDb
{
    public class TblBaseSoftDelete
    {
        public Guid Id { get; set; }
        public DateTimeOffset TimestampCreated { get; set; }
        public DateTimeOffset TimestampUpdated { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset? TimestampDeleted { get; set; }
    }
}
