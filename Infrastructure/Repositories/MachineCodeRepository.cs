using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Core.Interfaces;
using Core.Entities;

namespace Infrastructure.Repositories
{
    public class MachineCodeRepository : IMachineCodeRepository
    {
        private readonly ApplicationDbContext _context;

        public MachineCodeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MachineCode>> GetAllAsync()
        {
            return await _context.MachineCodes
                .Include(m => m.Process)
                    .ThenInclude(p => p.Line)
                .ToListAsync();
        }

        public async Task<MachineCode?> GetByIdAsync(int id)
        {
            return await _context.MachineCodes
                .Include(m => m.Process)
                    .ThenInclude(p => p.Line)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<MachineCode> AddAsync(MachineCode machineCode)
        {
            _context.MachineCodes.Add(machineCode);
            await _context.SaveChangesAsync();
            return machineCode;
        }

        public async Task UpdateAsync(MachineCode machineCode)
        {
            _context.MachineCodes.Update(machineCode);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(MachineCode machineCode)
        {
            _context.MachineCodes.Remove(machineCode);
            await _context.SaveChangesAsync();
        }
    }
}