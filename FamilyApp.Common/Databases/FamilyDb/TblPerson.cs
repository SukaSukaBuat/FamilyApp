using FamilyApp.Common.Databases.FamilyDb.Objects;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyApp.Common.Databases.FamilyDb
{
    public class TblPerson: TblBaseSoftDelete
    {
        public string Name { get; set; } = null!;
        public string? Nickname { get; set; } = null!;
        public Gender Gender { get; set; }
        public string? IcNumber { get; set; } = null!;
        public string? PhoneNumber { get; set; } = null!;

        [Column(TypeName = "jsonb")]
        public Address? Address { get; set; }
        [Comment("Reference to the Id of user, where it will get the address from the user as it address, if the column address do not have data")]
        public Guid ReferenceAddress { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
        [Column(TypeName = "jsonb")]
        public Address? PlaceOfBirth { get; set; }
        public bool IsAlive { get; set; } = true;
        public DateTimeOffset? DateOfDeath { get; set; }
        public Guid? UserId { get; set; }
        public virtual  TblUser? User { get; set; }
        public virtual ICollection<TblMarried> AsHusband { get; set; } = [];
        public virtual ICollection<TblMarried> AsWife { get; set; } = [];
    }
}
