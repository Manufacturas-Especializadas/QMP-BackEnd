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

        public async Task<int> GetMaxFolioAsync()
        {
            var maxFolio = await _context.Set<Rejection>()
                    .MaxAsync(r => (int?)r.Folio) ?? 0;

            return maxFolio;
        }

        public async Task<IEnumerable<RejectionResponse>> GetAllAsync(string? searchTerm)
        {
            var query = _context.Rejections
                        .Include(r => r.Defect)
                        .Include(r => r.Condition)
                        .Include(r => r.Line)
                        .Include(r => r.Client)
                        .Include(r => r.ContainmentAction)
                        .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(r =>
                    r.Folio.ToString()!.Contains(searchTerm) ||
                    r.PartNumber!.Contains(searchTerm)
                );
            }

            return await query
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new RejectionResponse(
                    r.Id,
                    r.Folio,
                    r.Inspector,
                    r.PartNumber!,
                    r.NumberOfPieces,
                    r.OperatorPayroll,
                    r.Description!,
                    r.Image,
                    r.InformedSignature,
                    r.CreatedAt,
                    r.Defect!.DefectName,             
                    r.Condition!.ConditionName,
                    r.Line!.LineName,
                    r.Client!.ClientName,
                    r.Inspector,
                    r.ContainmentAction!.ContainmentActionName,
                    r.DefectId,
                    r.ConditionId,
                    r.LineId,
                    r.ClientId,
                    r.ContainmentActionId
                ))
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetAvailableMonthsAsync()
        {
            return await _context.Rejections
                .Select(r => new { r.CreatedAt.Year, r.CreatedAt.Month })
                .Distinct()
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .Select(x => $"{new DateTime(x.Year, x.Month, 1):MMMM yyyy}")
                .ToListAsync();
        }
    }
}