using Infrastructure.Data;
using Core.Interfaces;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Core.DTOs;

namespace Infrastructure.Repositories
{
    public class AuditScrapRepository : IAuditScrapRepository
    {
        private readonly ApplicationDbContext _context;

        public AuditScrapRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AuditDataScrap?> GetByIdAsync(int id)
        {
            return await _context.AuditDataScraps
                        .Include(a => a.User)
                        .Include(a => a.Shift)
                        .Include(a => a.Lines)
                        .Include(a => a.Findings).ThenInclude(f => f.TypeScrap)
                        .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<AuditDataScrapReadDto>> GetAllDetailedAsync()
        {
            return await _context.AuditDataScraps
                .OrderByDescending(a => a.Id)
                .Select(a => new AuditDataScrapReadDto(
                    a.Id,
                    a.AuditDate,
                    a.UserId,
                    a.User.Username,
                    a.ShiftId,
                    a.Shift.ShiftName,
                    a.Lines.Select(l => l.LineName).ToList(),
                    a.Findings.Select(f => new AuditFindingScrapReadDto(
                        f.Id,
                        f.TypeScrapId,
                        f.TypeScrap.TypeScrapName,
                        f.EstimatedWeight,
                        f.MaterialCorrectlyIdentified,
                        f.MaterialCorrectlySegregated,
                        f.UnreportedReason,
                        f.ImageEvidence,
                        f.SupervisorSignature
                    )).ToList()
                ))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditDataScrap>> GetByMonthAsync(int month, int year)
        {
            return await _context.AuditDataScraps
                    .Include(a => a.User)
                    .Include(a => a.Shift)
                    .Include(a => a.Lines)
                    .Include(a => a.Findings).ThenInclude(f => f.TypeScrap)
                    .Where(a => a.AuditDate.Month == month && a.AuditDate.Year == year)
                    .OrderByDescending(a => a.AuditDate)
                    .ToListAsync();
        }

        public async Task<AuditDataScrap> CreateAsync(AuditDataScrap audit)
        {
            await _context.AuditDataScraps.AddAsync(audit);

            return audit;
        }

        public async Task<bool> UpdateAuditAsync(int id, UpdateAuditScrapDto dto, List<AuditFindingScrap> updatedFindings)
        {
            var audit = await _context.AuditDataScraps
                .Include(a => a.Lines)
                .Include(a => a.Findings)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (audit == null) return false;

            audit.ShiftId = dto.ShiftId;

            audit.Lines.Clear();
            var newLines = await _context.Lines.Where(l => dto.LineIds.Contains(l.Id)).ToListAsync();
            foreach (var line in newLines) audit.Lines.Add(line);

            var updatedFindingIds = updatedFindings.Where(f => f.Id > 0).Select(f => f.Id).ToList();
            var findingsToDelete = audit.Findings.Where(f => !updatedFindingIds.Contains(f.Id)).ToList();

            if (findingsToDelete.Any())
            {
                _context.AuditFindingsScraps.RemoveRange(findingsToDelete);
            }

            foreach (var incoming in updatedFindings)
            {
                if (incoming.Id > 0)
                {
                    var existingFinding = audit.Findings.FirstOrDefault(f => f.Id == incoming.Id);
                    if (existingFinding != null)
                    {
                        existingFinding.TypeScrapId = incoming.TypeScrapId;
                        existingFinding.EstimatedWeight = incoming.EstimatedWeight;
                        existingFinding.MaterialCorrectlyIdentified = incoming.MaterialCorrectlyIdentified;
                        existingFinding.MaterialCorrectlySegregated = incoming.MaterialCorrectlySegregated;
                        existingFinding.UnreportedReason = incoming.UnreportedReason;
                        existingFinding.ImageEvidence = incoming.ImageEvidence;
                        existingFinding.SupervisorSignature = incoming.SupervisorSignature;

                        _context.Entry(existingFinding).State = EntityState.Modified;
                    }
                }
                else
                {
                    incoming.AuditId = audit.Id;
                    await _context.AuditFindingsScraps.AddAsync(incoming);
                }
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var audit = await _context.AuditDataScraps
                        .Include(a => a.Findings)
                        .FirstOrDefaultAsync(a => a.Id == id);

            if (audit == null) return false;

            _context.AuditDataScraps.Remove(audit);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}