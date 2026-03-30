using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IRejectionRepository
    {
        Task<Rejection> GetByIdAsync(int id);

        Task<int> CreateAsync(Rejection rejection);

        Task UpdateAsync(Rejection rejection);

        Task<bool> UserExistsAsync(int userId);
    }
}