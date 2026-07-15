using Core.DTOs;

namespace Core.Interfaces
{
    public interface IAuditFcdsRepository
    {
        Task<bool> CreateAuditAsync(CreateAuditFcdsDto dto, int userId);

        Task<bool> UpdateAuditAsync(int id, CreateAuditFcdsDto dto);

        Task<bool> DeleteAuditAsync(int id);

        Task<PagedResponse<AuditFcdsListDto>> GetListAuditsAsync(PaginationParams paginationParams);

        Task<DetailedAuditFcdsDto?> GetDetailedAuditByIdAsync(int id);

        Task<IEnumerable<DetailedAuditFcdsDto>> GetAuditsByMonthAsync(int year, int month);

        Task<IEnumerable<AvailableMonthDto>> GetAvailableMonthsAsync();
    }
}