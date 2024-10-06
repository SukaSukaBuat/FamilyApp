using FamilyApp.Common.Databases.FamilyDb.Objects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyApp.Common.Databases.FamilyDb
{
    public class TblMarried: TblBase
    {
        public Guid HusbandId { get; set; }
        public TblPerson Husband { get; set; } = null!;
        public Guid WifeId { get; set; }
        public TblPerson Wife { get; set; } = null!;
        [Comment("Children id and relationship from table tbl_person")]
        [Column(TypeName ="jsonb")]
        public Children[] Children { get; set; } = [];
        public DateOnly? DateOfMarried { get; set; }
    }
}
