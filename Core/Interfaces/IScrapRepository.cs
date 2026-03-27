using Core.DTOs;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IScrapRepository
    {
        Task<Scrap?> GetByIdAsync(int id);

        Task<IEnumerable<Scrap>> GetAllTodayAsync();

        Task<IEnumerable<Scrap>> GetByMonthAsync(int month, int year);

        Task<Scrap> CreateAsync(Scrap scrap);

        Task<bool> UpdateVerificationAsync(int id, bool isVerified, decimal? verifiedWeight);

        Task<bool> SaveChangesAsync();
    }
}