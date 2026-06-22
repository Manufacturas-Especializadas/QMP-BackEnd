using Core.Entities;
using Core.DTOs;

namespace Core.Interfaces
{
    public interface IAuditScrapRepository
    {
        Task<AuditDataScrap?> GetByIdAsync(int id);

        Task<IEnumerable<AuditDataScrapReadDto>> GetAllDetailedAsync();

        Task<IEnumerable<AuditDataScrap>> GetByMonthAsync(int month, int year);

        Task<AuditDataScrap> CreateAsync(AuditDataScrap entity);

        Task<bool> DeleteAsync(int id);

        Task<bool> SaveChangesAsync();
    }
}