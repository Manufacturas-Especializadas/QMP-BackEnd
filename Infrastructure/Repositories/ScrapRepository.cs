using Core.DTOs;
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
    public class ScrapRepository : IScrapRepository
    {
        private readonly ApplicationDbContext _context;

        public ScrapRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Scrap?> GetByIdAsync(int id)
        {
            return await _context.Scraps
                .Include(s => s.Line)
                .Include(s => s.Process)
                .Include(s => s.TypeScrap)
                .Include(s => s.Defect)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Scrap>> GetByMonthAsync(int month, int year)
        {
            return await _context.Scraps
                .Include(s => s.Line)
                .Include(s => s.Shift)
                .Include(s => s.Process)
                .Include(s => s.MachineCode)
                .Include(s => s.TypeScrap)
                .Include(s => s.Defect)
                .Where(s => s.CreatedAt.Month == month && s.CreatedAt.Year == year)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ScrapReadDto>> GetAllAsync()
        {
            return await _context.Scraps
                    .OrderByDescending(s => s.Id)
                    .Select(s => new ScrapReadDto(
                        s.Id,
                        s.PayRollNumber,
                        s.Alloy,
                        s.Diameter,
                        s.Wall,
                        s.RDM,
                        s.Weight,
                        s.CreatedAt,
                        s.Shift.ShiftName,
                        s.Line.LineName,
                        s.Process!.ProcessName,
                        s.MachineCode != null ? s.MachineCode.MachineCodeName : "N/A",
                        s.TypeScrap.TypeScrapName,
                        s.Defect.DefectName,
                        s.IsVerified,
                        s.VerifiedWeight
                    ))
                    .AsNoTracking()
                    .ToListAsync();
        }

        public async Task<Scrap> CreateAsync(Scrap scrap)
        {
            await _context.Scraps.AddAsync(scrap);

            return scrap;
        }

        public async Task<bool> UpdateVerificationAsync(int id, bool isVerified, decimal? verifiedWeight)
        {
            var scrap = await _context.Scraps.FindAsync(id);
            if (scrap == null) return false;

            scrap.IsVerified = isVerified;
            scrap.VerifiedWeight = isVerified ? scrap.Weight : verifiedWeight;

            _context.Entry(scrap).State = EntityState.Modified;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}