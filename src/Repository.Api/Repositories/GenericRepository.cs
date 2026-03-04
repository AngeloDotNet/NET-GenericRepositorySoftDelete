using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Repository.Api.Data;
using Repository.Api.Entities;
using Repository.Api.Repositories.Interfaces;

namespace Repository.Api.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext dbContext;
    protected readonly DbSet<T> dbSet;

    public GenericRepository(AppDbContext context)
    {
        dbContext = context;
        dbSet = dbContext.Set<T>();
    }

    public virtual IQueryable<T> Query() => dbSet.AsQueryable();

    public virtual async Task<T?> GetByIdAsync(int id)
        => await dbSet.FirstOrDefaultAsync(e => e.Id == id); // usa FirstOrDefault per rispettare i query filters globali

    public virtual async Task<IEnumerable<T>> GetAllAsync()
        => await dbSet.ToListAsync();

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await dbSet.Where(predicate).ToListAsync();

    public virtual async Task AddAsync(T entity)
        => await dbSet.AddAsync(entity);

    public virtual void Update(T entity)
        => dbSet.Update(entity);

    public virtual Task DeleteAsync(T entity)
    {
        // Quando SaveChanges viene chiamato, il DbContext trasformerà lo stato Deleted
        // in soft-delete tramite ApplySoftDelete, ma qui possiamo anche settare lo stato Deleted direttamente
        dbContext.Entry(entity).State = EntityState.Deleted;
        return Task.CompletedTask;
    }

    public virtual async Task HardDeleteAsync(T entity)
    {
        dbSet.Remove(entity);
        await Task.CompletedTask;
    }
}