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
                .Include(s => s.Shift)
                .Include(s => s.Line)
                .Include(s => s.Process)
                .Include(s => s.MachineCode)
                .Include(s => s.ScrapDetails)
                    .ThenInclude(d => d.Material)
                .Include(s => s.ScrapDetails)
                    .ThenInclude(d => d.TypeScrap)
                .Include(s => s.ScrapDetails)
                    .ThenInclude(d => d.Defect)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Scrap>> GetByMonthAsync(int month, int year)
        {
            return await _context.Scraps
                .Include(s => s.Line)
                .Include(s => s.Shift)
                .Include(s => s.Process)
                .Include(s => s.MachineCode)
                .Include(s => s.ScrapDetails)
                    .ThenInclude(d => d.TypeScrap)
                .Include(s => s.ScrapDetails)
                    .ThenInclude(d => d.Defect)
                .Include(s => s.ScrapDetails)
                    .ThenInclude(d => d.Material)
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
                    s.CreatedAt,
                    s.Shift.ShiftName,
                    s.Line.LineName,
                    s.Process != null ? s.Process.ProcessName : "N/A",
                    s.MachineCode != null ? s.MachineCode.MachineCodeName : "N/A",
                    s.ScrapDetails.Select(d => new ScrapDetailReadDto(
                        d.Id,
                        d.Alloy,
                        d.Diameter,
                        d.Wall,
                        d.RDM,
                        d.Weight,
                        d.Material.MaterialName,
                        d.TypeScrap.TypeScrapName,
                        d.Defect != null ? d.Defect.DefectName : "N/A",
                        d.IsVerified,
                        d.VerifiedWeight
                    )).ToList()
                ))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Scrap> CreateAsync(Scrap scrap)
        {
            await _context.Scraps.AddAsync(scrap);

            return scrap;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var scrap = await _context.Scraps
                 .Include(s => s.ScrapDetails)
                 .FirstOrDefaultAsync(s => s.Id == id);

            if (scrap == null) return false;

            _context.Scraps.Remove(scrap);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(int id, Scrap updatedScrap, List<ScrapDetail> newDetails)
        {
            var existingScrap = await _context.Scraps
                .Include(s => s.ScrapDetails)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (existingScrap == null) return false;

            existingScrap.PayRollNumber = updatedScrap.PayRollNumber;
            existingScrap.ShiftId = updatedScrap.ShiftId;
            existingScrap.ProcessId = updatedScrap.ProcessId;
            existingScrap.LineId = updatedScrap.LineId;
            existingScrap.MachineCodeId = updatedScrap.MachineCodeId;

            var incomingDetailIds = newDetails.Where(d => d.Id > 0).Select(d => d.Id).ToList();
            var detailsToRemove = existingScrap.ScrapDetails
                .Where(d => !incomingDetailIds.Contains(d.Id))
                .ToList();

            _context.ScrapDetails.RemoveRange(detailsToRemove);

            foreach (var detail in newDetails)
            {
                if (detail.Id == 0)
                {
                    existingScrap.ScrapDetails.Add(detail);
                }
                else
                {
                    var existingDetail = existingScrap.ScrapDetails.FirstOrDefault(d => d.Id == detail.Id);
                    if (existingDetail != null)
                    {
                        existingDetail.Alloy = detail.Alloy;
                        existingDetail.Diameter = detail.Diameter;
                        existingDetail.Wall = detail.Wall;
                        existingDetail.RDM = detail.RDM;
                        existingDetail.Weight = detail.Weight;
                        existingDetail.MaterialId = detail.MaterialId;
                        existingDetail.TypeScrapId = detail.TypeScrapId;
                        existingDetail.DefectId = detail.DefectId;
                    }
                }
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateVerificationAsync(int detailId, bool isVerified, decimal? verifiedWeight)
        {
            
            var detail = await _context.ScrapDetails.FindAsync(detailId);
            if (detail == null) return false;

            detail.IsVerified = isVerified;
            detail.VerifiedWeight = isVerified ? detail.Weight : verifiedWeight;

            _context.Entry(detail).State = EntityState.Modified;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}