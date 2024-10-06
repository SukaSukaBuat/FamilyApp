using FamilyApp.Common.Databases.FamilyDb;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FamilyApp.Common.Repositories
{
    public interface IGenericRepository
    {
        Task<int> SaveChanges(Guid? userId = null);
    }

    public class GenericRepository(FamilyDbContext context) : IGenericRepository
    {

        public async Task<int> SaveChanges(Guid? userId = null)
        {
            var entries = Enumerable.ToList(context.ChangeTracker.Entries());
            var jsonFormatting = new JsonSerializerOptions { WriteIndented = false };
            foreach (var entry in entries)
            {
                //only if entity implement softdelete
                if (entry.Entity != null && entry.Entity.GetType().IsSubclassOf(typeof(TblBaseSoftDelete)))
                {
                    var entity = entry.Entity as TblBaseSoftDelete;
                    if (entity != null)
                    {
                        //instead of delete, it will flag the row as deleted
                        if (entry.State == EntityState.Deleted)
                        {
                            entry.State = EntityState.Modified;
                            entity.IsDeleted = true;
                            entity.TimestampDeleted = DateTimeOffset.UtcNow;
                            entity.TimestampUpdated = DateTimeOffset.UtcNow;
                        }
                        //generate id and related timestamp
                        else if (entry.State == EntityState.Added)
                        {
                            entity.Id = Guid.NewGuid();
                            entity.TimestampUpdated = DateTimeOffset.UtcNow;
                            entity.TimestampCreated = DateTimeOffset.UtcNow;
                        }
                        //update timestamp updated
                        else if (entry.State == EntityState.Modified)
                        {
                            entity.TimestampUpdated = DateTimeOffset.UtcNow;
                        }
                    }
                }
                //if not implement soft delete
                else if (entry.Entity != null && entry.Entity.GetType().IsSubclassOf(typeof(TblBase)))
                {
                    var entity = entry.Entity as TblBase;
                    if (entity is not null)
                    {
                        //generate id and related timestamp
                        if (entry.State == EntityState.Added)
                        {
                            entity.Id = Guid.NewGuid();
                            entity.TimestampUpdated = DateTimeOffset.UtcNow;
                            entity.TimestampCreated = DateTimeOffset.UtcNow;
                        }
                        //update timestamp updated
                        else if (entry.State == EntityState.Modified)
                        {
                            entity.TimestampUpdated = DateTimeOffset.UtcNow;
                        }
                    }
                }
                //if user id was passed during savechange, add action to audit trail
                if (userId is not null && entry.Entity is not null)
                {
                    var auditTrail = new TblAuditTrail
                    {
                        Id = Guid.NewGuid(),
                        Action = entry.State,
                        ActionTimestamp = DateTimeOffset.UtcNow,
                        ActorId = userId.Value,
                        TableName = entry.Entity.GetType().Name,
                        Data = JsonSerializer.Serialize(entry.Entity, jsonFormatting)
                    };
                    //only do this for tbl that implement either TblBaseSoftDelete or TblBase
                    if (entry.Entity.GetType().IsSubclassOf(typeof(TblBaseSoftDelete)))
                    {
                        var entity = entry.Entity as TblBaseSoftDelete;
                        if (entity is not null)
                            auditTrail.TableId = entity.Id;
                    }
                    else if (entry.Entity.GetType().IsSubclassOf(typeof(TblBase)))
                    {
                        var entity = entry.Entity as TblBase;
                        if (entity is not null)
                            auditTrail.TableId = entity.Id;
                    }
                    context.TblAuditTrail.Add(auditTrail);
                }
            }
            return await context.SaveChangesAsync();
        }
    }
}
