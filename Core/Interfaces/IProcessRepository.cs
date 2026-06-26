using Core.Entities;

namespace Core.Interfaces
{
    public interface IProcessRepository
    {
        Task<IEnumerable<Process>> GetAllAsync();

        Task<Process?> GetByIdAsync(int id);

        Task<Process> AddAsync(Process process);

        Task UpdateAsync(Process process);

        Task DeleteAsync(Process process);
    }
}