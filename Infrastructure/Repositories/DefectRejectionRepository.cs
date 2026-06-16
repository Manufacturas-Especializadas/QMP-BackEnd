using Core.Interfaces;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class DefectRejectionRepository /*: IDefectRejectionRepository*/
    {
        private readonly ApplicationDbContext _context;

        public DefectRejectionRepository(ApplicationDbContext context)
        {
            _context = context;
        }


    }
}