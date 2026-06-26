using Core.Entities;

namespace Core.Interfaces
{
    public interface IMachineCodeRepository
    {
        Task<IEnumerable<MachineCode>> GetAllAsync();

        Task<MachineCode?> GetByIdAsync(int id);

        Task<MachineCode> AddAsync(MachineCode machineCode);

        Task UpdateAsync(MachineCode machineCode);

        Task DeleteAsync(MachineCode machineCode);
    }
}