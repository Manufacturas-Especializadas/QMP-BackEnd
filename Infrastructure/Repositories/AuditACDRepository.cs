using Infrastructure.Data;
using Core.Interfaces;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Core.DTOs;

namespace Infrastructure.Repositories
{
    public class AuditACDRepository : IAuditACDRepository
    {
        private readonly ApplicationDbContext _context;

        public AuditACDRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AuditDataACD?> GetByIdAsync(int id)
        {
            return await _context.AuditDataACDs
                        .Include(a => a.User)
                        .Include(a => a.Shift)
                        .Include(a => a.Lines)
                        .Include(a => a.Rejection)
                        .Include(a => a.Findings).ThenInclude(f => f.StartPoint)
                        .Include(a => a.Findings).ThenInclude(f => f.EndPoint)
                        .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<AuditDataACDReadDto>> GetAllDetailedAsync()
        {
            return await _context.AuditDataACDs
                        .OrderByDescending(a => a.Id)
                        .Select(a => new AuditDataACDReadDto(
                            a.Id,
                            a.AuditDate,
                            a.UserId,
                            a.User.Username,
                            a.ShiftId,
                            a.Shift.ShiftName,
                            a.RejectionId,
                            a.Rejection != null ? a.Rejection.Folio : null,
                            a.Lines.Select(l => l.LineName).ToList(),
                            a.Lines.Select(l => l.Id).ToList(),
                            a.Findings.Select(f => new AuditFindingACDReadDto(
                                f.Id,
                                f.StartPointId,
                                f.StartPoint.ProcessName,
                                f.EndPointId,
                                f.EndPoint.ProcessName,
                                f.PartNumber,
                                f.NumberOfPieces,
                                f.SampleSize,
                                f.PackerPayroll,
                                f.ContainerIdMatch,
                                f.FrontView,
                                f.SideView,
                                f.TopView,
                                f.IsometricView,
                                f.CompleteProcess,
                                f.IsProductConforming,
                                f.ShopOrder,
                                f.WeldingDefects,
                                f.PpBom,
                                f.ImagesEvidence
                            )).ToList()
                        ))
                        .AsNoTracking()
                        .ToListAsync();
        }

        public async Task<IEnumerable<AuditDataACD>> GetByMonthAsync(int month, int year)
        {
            return await _context.AuditDataACDs
                .Include(a => a.User)
                .Include(a => a.Shift)
                .Include(a => a.Lines)
                .Include(a => a.Findings).ThenInclude(f => f.StartPoint)
                .Include(a => a.Findings).ThenInclude(f => f.EndPoint)
                .Where(a => a.AuditDate.Month == month && a.AuditDate.Year == year)
                .OrderByDescending(a => a.AuditDate)
                .ToListAsync();
        }

        public async Task<AuditDataACD> CreateAsync(AuditDataACD audit)
        {
            await _context.AuditDataACDs.AddAsync(audit);

            return audit;
        }

        public async Task<bool> UpdateAuditAsync(int id, UpdateAuditACDDto dto, List<AuditFindingACD> updatedFindings)
        {
            var audit = await _context.AuditDataACDs
                    .Include(a => a.Lines)
                    .Include(a => a.Findings)
                    .FirstOrDefaultAsync(a => a.Id == id);

            if (audit == null) return false;

            audit.ShiftId = dto.ShiftId;
            audit.RejectionId = dto.RejectionId;

            audit.Lines.Clear();
            var newLines = await _context.Lines.Where(l => dto.LineIds.Contains(l.Id)).ToListAsync();

            foreach (var line in newLines) audit.Lines.Add(line);

            var updatedFindingIds = updatedFindings.Where(f => f.Id > 0).Select(f => f.Id).ToList();
            var findingsToDelete = audit.Findings.Where(f => !updatedFindingIds.Contains(f.Id)).ToList();

            if (findingsToDelete.Any())
            {
                _context.AuditFindingsACDs.RemoveRange(findingsToDelete);
            }

            foreach (var incoming in updatedFindings)
            {
                if (incoming.Id > 0)
                {
                    var existingFinding = audit.Findings.FirstOrDefault(f => f.Id == incoming.Id);

                    if (existingFinding != null)
                    {
                        existingFinding.StartPointId = incoming.StartPointId;
                        existingFinding.EndPointId = incoming.EndPointId;
                        existingFinding.PartNumber = incoming.PartNumber;
                        existingFinding.NumberOfPieces = incoming.NumberOfPieces;
                        existingFinding.SampleSize = incoming.SampleSize;
                        existingFinding.PackerPayroll = incoming.PackerPayroll;
                        existingFinding.ContainerIdMatch = incoming.ContainerIdMatch;
                        existingFinding.FrontView = incoming.FrontView;
                        existingFinding.SideView = incoming.SideView;
                        existingFinding.TopView = incoming.TopView;
                        existingFinding.IsometricView = incoming.IsometricView;
                        existingFinding.CompleteProcess = incoming.CompleteProcess;
                        existingFinding.IsProductConforming = incoming.IsProductConforming;

                        _context.Entry(existingFinding).State = EntityState.Modified;
                    }
                }
                else
                {
                    incoming.AuditId = audit.Id;
                    await _context.AuditFindingsACDs.AddAsync(incoming);
                }
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var audit = await _context.AuditDataACDs.FindAsync(id);

            if (audit == null) return false;

            _context.AuditDataACDs.Remove(audit);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}