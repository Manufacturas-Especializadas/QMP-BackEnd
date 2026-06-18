using Core.DTOs;

namespace Core.Interfaces
{
    public interface IAuditFcdsRepository
    {
        Task<bool> CreateAuditAsync(CreateAuditFcdsDto dto, int userId);

        Task<bool> DeleteAuditAsync(int id);

        Task<IEnumerable<AuditFcdsListDto>> GetListAuditsAsync();

        Task<DetailedAuditFcdsDto?> GetDetailedAuditByIdAsync(int id);
    }
}