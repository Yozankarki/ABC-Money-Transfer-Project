using Microsoft.EntityFrameworkCore.Storage;

namespace MoneyTransfer.Application.Interfaces
{
    public interface IRepoService<T> where T : class
    {
        IQueryable<T> GetAll();
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<T?> GetByIdAsync(string id);
        Task<T?> GetByUserIdAsync(string userId);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
