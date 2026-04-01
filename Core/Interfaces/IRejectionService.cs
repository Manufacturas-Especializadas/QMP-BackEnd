using Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IRejectionService
    {
        Task<int> CreateRejectionAsync(CreateRejectionDto dto, int userId);

        Task UpdateRejectionAsync(EditRejectionDto dto);

        Task<int> GetNextFolioAsync();

        Task<IEnumerable<RejectionResponse>> GetAllAsync(string? searchTerm);

        Task<IEnumerable<string>> GetAvailableMonthsAsync();

        Task<IEnumerable<RejectionResponse>> GetByMonthAsync(string monthYear);
    }
}