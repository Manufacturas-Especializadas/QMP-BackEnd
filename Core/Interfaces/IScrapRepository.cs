using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IScrapRepository
    {
        Task<Scrap> CreateAsync(Scrap scrap);

        Task<bool> SaveChangesAsync();
    }
}