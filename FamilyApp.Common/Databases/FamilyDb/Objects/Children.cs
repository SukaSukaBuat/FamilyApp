using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyApp.Common.Databases.FamilyDb.Objects
{
    public enum ChildRelationshipType
    {
        Biological,
        Adopted,
        Step
    }
    public class Children
    {
        public Guid PersonId { get; set; }
        public ChildRelationshipType RelationshipType { get; set; }
    }
}
