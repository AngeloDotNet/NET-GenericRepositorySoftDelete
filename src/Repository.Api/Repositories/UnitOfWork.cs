using System.Collections.Concurrent;
using Repository.Api.Data;
using Repository.Api.Entities;
using Repository.Api.Repositories.Interfaces;

namespace Repository.Api.Repositories;

public class UnitOfWork(AppDbContext context) : IUnitOfWork, IDisposable
{
    private readonly ConcurrentDictionary<Type, object> repositories = new();

    public IGenericRepository<T> Repository<T>() where T : BaseEntity
    {
        var type = typeof(T);

        if (!repositories.ContainsKey(type))
        {
            var repoType = typeof(GenericRepository<>).MakeGenericType(type);
            var repoInstance = Activator.CreateInstance(repoType, context)!;

            repositories[type] = repoInstance;
        }

        return (IGenericRepository<T>)repositories[type]!;
    }

    public Task<int> SaveChangesAsync()
        => context.SaveChangesAsync();

    public void Dispose()
    {
        context.Dispose();
    }
}