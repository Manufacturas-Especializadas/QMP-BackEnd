using Core.DTOs;

namespace Core.Interfaces
{
    public interface IAuditFcdsRepository
    {
        Task<bool> CreateAuditAsync(CreateAuditFcdsDto dto, int userId);

        Task<IEnumerable<AuditFcdsListDto>> GetListAuditsAsync();
    }
}