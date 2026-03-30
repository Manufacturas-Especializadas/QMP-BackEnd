using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class RejectionRepository : IRejectionRepository
    {
        private readonly ApplicationDbContext _context;

        public RejectionRepository(ApplicationDbContext context) => _context = context;

        public async Task<Rejection?> GetByIdAsync(int id)
        {
             return await _context.Set<Rejection>().FindAsync(id);
        }

        public async Task<int> CreateAsync(Rejection rejection)
        {
            _context.Set<Rejection>().Add(rejection);

            await _context.SaveChangesAsync();

            return rejection.Id;
        }

        public async Task UpdateAsync(Rejection rejection)
        {
            _context.Set<Rejection>().Update(rejection);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> UserExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId);
        }
    }
}