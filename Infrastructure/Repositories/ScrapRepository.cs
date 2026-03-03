using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
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

        public async Task<Scrap> CreateAsync(Scrap scrap)
        {
            await _context.Scraps.AddAsync(scrap);

            return scrap;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}