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
    public class ListRepository : IListRepository
    {
        private readonly ApplicationDbContext _context;

        public ListRepository(ApplicationDbContext context)
        {
            _context = context;            
        }

        public async Task<IEnumerable<LineLookupDto>> GetLines()
        {
            return await _context.Lines
                    .AsNoTracking()
                    .Select(l => new LineLookupDto(l.Id, l.LineName))
                    .ToListAsync();
        }

        public async Task<IEnumerable<ShiftLookupDto>> GetShifts()
        {
            return await _context.Shifts
                        .AsNoTracking()
                        .Select(s => new ShiftLookupDto(s.Id, s.ShiftName))
                        .ToListAsync();
        }

        public async Task<IEnumerable<ProcessLookupDto>> GetProcess(int lineId)
        {
            return await _context.Processes
                        .AsNoTracking()
                        .Where(p => p.LineId == lineId)
                        .Select(p => new ProcessLookupDto(p.Id, p.ProcessName, p.LineId))
                        .ToListAsync();

        }
    }
}
