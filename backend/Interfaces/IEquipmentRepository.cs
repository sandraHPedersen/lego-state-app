using RedYellowGreen.Api.Data.Models;
using RedYellowGreen.Api.Data.Models.Entities;
namespace RedYellowGreen.Api.Interfaces
{
    public interface IEquipmentRepository
    {
        Task<List<Equipment>> GetAllAsync();
        Task<Equipment?> GetByIdAsync(int id);
        Task<List<StateChange>> GetHistoryAsync(int equipmentId, int limit = 200);
        Task<List<ProductionOrder>> GetOrdersAsync(int equipmentId);
        Task<ProductionOrder?> GetCurrentOrderAsync(int equipmentId);
        Task AddStateChangeAsync(StateChange change);
        Task SaveChangesAsync();
    }
}
