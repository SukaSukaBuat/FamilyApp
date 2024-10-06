using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyApp.Common.Databases.FamilyDb
{
    public class TblLoginSession: TblBase
    {
        public Guid UserId { get; set; }
        public virtual TblUser TblUser { get; set; }
    }
}
