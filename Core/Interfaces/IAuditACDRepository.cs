using Core.Entities;
using Core.DTOs;

namespace Core.Interfaces
{
    public interface IAuditACDRepository
    {
        Task<AuditDataACD?> GetByIdAsync(int id);

        Task<IEnumerable<AuditDataACDReadDto>> GetAllDetailedAsync();

        Task<IEnumerable<AuditDataACD>> GetByMonthAsync(int month, int year);

        Task<AuditDataACD> CreateAsync(AuditDataACD audit);

        Task<bool> UpdateAuditAsync(int id, UpdateAuditACDDto dto, List<AuditFindingACD> updatedFindings);

        Task<bool> DeleteAsync(int id);

        Task<bool> SaveChangesAsync();
    }
}