using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Core.Interfaces;
using Core.Entities;

namespace Infrastructure.Repositories
{
    public class ProcessRepository : IProcessRepository
    {
        private readonly ApplicationDbContext _context;

        public ProcessRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Process>> GetAllAsync()
        {
            return await _context.Processes
                .Include(p => p.Line)
                .ToListAsync();
        }

        public async Task<Process?> GetByIdAsync(int id)
        {
            return await _context.Processes
                .Include(p => p.Line)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Process> AddAsync(Process process)
        {
            _context.Processes.Add(process);
            await _context.SaveChangesAsync();
            return process;
        }

        public async Task UpdateAsync(Process process)
        {
            _context.Processes.Update(process);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Process process)
        {
            _context.Processes.Remove(process);
            await _context.SaveChangesAsync();
        }
    }
}