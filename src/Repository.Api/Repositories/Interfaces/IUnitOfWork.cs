using Repository.Api.Entities;

namespace Repository.Api.Repositories.Interfaces;

public interface IUnitOfWork
{
    IGenericRepository<T> Repository<T>() where T : BaseEntity;
    Task<int> SaveChangesAsync();
}