using System;
using System.Threading.Tasks;
using Assignment_Example_HU.Models;

namespace Assignment_Example_HU.Repositories.Interfaces
{
    public interface IGamePlayerRepository
    {
        Task AddAsync(GamePlayer gamePlayer);
        Task RemoveAsync(GamePlayer gamePlayer);
        Task SaveChangesAsync();
    }
}
