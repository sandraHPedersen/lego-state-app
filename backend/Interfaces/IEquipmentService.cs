using RedYellowGreen.Api.Data.Models.Entities;

namespace RedYellowGreen.Api.Interfaces
{
    public interface IEquipmentService
    {
        Task<List<Equipment>> GetAllAsync();
        Task<Equipment?> GetByIdAsync(int id);
        Task<List<StateChange>> GetHistoryAsync(int id);
        Task<List<ProductionOrder>> GetOrdersAsync(int id);
        Task<ProductionOrder?> GetCurrentOrderAsync(int id);
        Task<Equipment?> UpdateStateAsync(int equipmentId, ProductionState newState, string changedBy);
    }
}
