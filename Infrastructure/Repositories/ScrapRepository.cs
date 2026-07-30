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
                .Include(s => s.ScrapDetails)
                    .ThenInclude(d => d.Process)
                .Include(s => s.ScrapDetails)
        .           ThenInclude(d => d.MachineCode)
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
                .Include(s => s.ScrapDetails)
                    .ThenInclude(d => d.Process)
                .Include(s => s.ScrapDetails)
                    .ThenInclude(d => d.MachineCode)
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
                    s.InspectorPayRollNumber,
                    s.CreatedAt,
                    s.ShiftId,
                    s.Shift.ShiftName,
                    s.LineId,
                    s.Line.LineName,
                    s.IsVerified,
                    s.VerifiedWeight,
                    s.ScrapDetails.Select(d => new ScrapDetailReadDto(
                            d.Id,
    d.PayRollNumber,
    d.ProcessId,
    d.Process != null ? d.Process.ProcessName : "N/A",
    d.MachineCodeId,
    d.MachineCode != null ? d.MachineCode.MachineCodeName : "N/A",
    d.Alloy,
    d.Diameter,
    d.Wall,
    d.RDM,
    d.Weight,
    d.MaterialId,
    d.Material != null ? d.Material.MaterialName : "N/A",
    d.TypeScrapId,
    d.TypeScrap != null ? d.TypeScrap.TypeScrapName : "N/A",
    d.DefectId,
    d.Defect != null ? d.Defect.DefectName : "N/A",
    d.PartNumber
                    )).ToList()
                ))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Scrap> CreateAsync(Scrap scrap)
        {
            scrap.TotalWeight = scrap.ScrapDetails.Sum(d => d.Weight ?? 0);
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

        public async Task<bool> UpdateDetailsOnlyAsync(int scrapId, List<ScrapDetailUpdateDto> newDetailsDto)
        {
            var existingScrap = await _context.Scraps
                .Include(s => s.ScrapDetails)
                .FirstOrDefaultAsync(s => s.Id == scrapId);

            if (existingScrap == null) return false;

            var incomingDetailIds = newDetailsDto.Where(d => d.Id > 0).Select(d => d.Id).ToList();

            var detailsToRemove = existingScrap.ScrapDetails
                .Where(d => !incomingDetailIds.Contains(d.Id))
                .ToList();
            _context.ScrapDetails.RemoveRange(detailsToRemove);

            foreach (var dto in newDetailsDto)
            {
                if (dto.Id == 0)
                {
                    existingScrap.ScrapDetails.Add(new ScrapDetail
                    {
                        PayRollNumber = dto.PayRollNumber,
                        ProcessId = dto.ProcessId,
                        MachineCodeId = dto.MachineCodeId,
                        Alloy = dto.Alloy,
                        Diameter = dto.Diameter,
                        Wall = dto.Wall,
                        RDM = dto.RDM,
                        Weight = dto.Weight,
                        MaterialId = dto.MaterialId,
                        TypeScrapId = dto.TypeScrapId,
                        DefectId = dto.DefectId,
                        PartNumber = dto.PartNumber
                    });
                }
                else
                {
                    var existingDetail = existingScrap.ScrapDetails.FirstOrDefault(d => d.Id == dto.Id);
                    if (existingDetail != null)
                    {
                        
                        existingDetail.PayRollNumber = dto.PayRollNumber;
                        existingDetail.ProcessId = dto.ProcessId;
                        existingDetail.MachineCodeId = dto.MachineCodeId;
                        existingDetail.Alloy = dto.Alloy;
                        existingDetail.Diameter = dto.Diameter;
                        existingDetail.Wall = dto.Wall;
                        existingDetail.RDM = dto.RDM;
                        existingDetail.Weight = dto.Weight;
                        existingDetail.MaterialId = dto.MaterialId;
                        existingDetail.TypeScrapId = dto.TypeScrapId;
                        existingDetail.DefectId = dto.DefectId;
                        existingDetail.PartNumber = dto.PartNumber;
                    }
                }
            }

            existingScrap.TotalWeight = existingScrap.ScrapDetails.Sum(d => d.Weight ?? 0);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateVerificationAsync(int scrapId, bool isVerified, decimal? verifiedWeight)
        {
            var scrap = await _context.Scraps.FindAsync(scrapId);
            if (scrap == null) return false;

            scrap.IsVerified = isVerified;
            scrap.VerifiedWeight = isVerified ? scrap.TotalWeight : verifiedWeight;

            _context.Entry(scrap).State = EntityState.Modified;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}