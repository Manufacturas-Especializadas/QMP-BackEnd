using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface ILinesRepository
    {
        Task<Line> CreateAsync(Line line);

        Task<bool> SaveChangesAsync();
    }
}