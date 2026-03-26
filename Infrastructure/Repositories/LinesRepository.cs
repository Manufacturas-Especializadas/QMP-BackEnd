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
    public class LinesRepository : ILinesRepository
    {
        private readonly ApplicationDbContext _context;

        public LinesRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Line> GetByIdAsync(int id)
        {
            return await _context.Lines.FindAsync(id);
        }

        public async Task<Line> CreateAsync(Line line)
        {
            await _context.Lines.AddAsync(line);

            return line;
        }

        public async Task<Line> UpdateAsync(int id, Line line)
        {
            var existingLine = await _context.Lines.FindAsync(id);

            if (existingLine == null) return null;

            existingLine.LineName = line.LineName;

            return existingLine;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var line = await _context.Lines.FindAsync(id);

            if(line == null) return false;

            _context.Lines.Remove(line);

            return true;
        } 

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
