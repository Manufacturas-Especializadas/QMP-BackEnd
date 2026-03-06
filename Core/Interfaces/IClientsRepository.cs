using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IClientsRepository
    {
        Task<Client> GetByIdAsync(int id);

        Task<Client> CreateAsync(Client client);

        Task<Client> UpdateAsync(int id, Client client);

        Task<bool> DeleteAsync(int id);

        Task<bool> SaveChangesAsync();
    }
}