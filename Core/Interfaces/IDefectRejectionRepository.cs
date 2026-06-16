using Core.Entities;


namespace Core.Interfaces
{
    public interface IDefectRejectionRepository
    {
        Task<DefectRejection> GetByIdAsync(int id);

        Task<DefectRejection> CreateAsync(DefectRejection defect);

        Task<DefectRejection> UpdateAsync(int id, DefectRejection defect);

        Task<bool> DeleteAsync(int id);

        Task<bool> SaveChangesAsync();
    }
}
