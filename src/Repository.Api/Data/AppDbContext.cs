using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Repository.Api.Entities;
using Repository.Api.Entities.Interfaces;

namespace Repository.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Applica global query filter a tutte le entità che implementano ISoftDelete
        var softDeleteInterface = typeof(ISoftDelete);
        var entityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(t => softDeleteInterface.IsAssignableFrom(t.ClrType))
            .Select(t => t.ClrType);

        foreach (var type in entityTypes)
        {
            // costruisci lambda: (e) => EF.Property<bool>(e, "IsDeleted") == false
            var parameter = Expression.Parameter(type, "e");
            var prop = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
            var condition = Expression.Lambda(Expression.Equal(prop, Expression.Constant(false)), parameter);

            modelBuilder.Entity(type).HasQueryFilter(condition);
        }
    }

    public override int SaveChanges()
    {
        ApplySoftDelete();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplySoftDelete();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplySoftDelete()
    {
        foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted))
        {
            if (entry.Entity is ISoftDelete sd)
            {
                // converti in soft delete
                sd.IsDeleted = true;
                sd.DeletedAt = DateTime.UtcNow;
                entry.State = EntityState.Modified;
            }
        }
    }
}