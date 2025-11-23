using RedYellowGreen.Api.Data.Models.Entities;
using RedYellowGreen.Api.Hubs;
using RedYellowGreen.Api.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace RedYellowGreen.Api.Services
{
    public class EquipmentService(IEquipmentRepository repo, IHubContext<StateHub> hub) : IEquipmentService
    {
        private readonly IEquipmentRepository _repo = repo;
        private readonly IHubContext<StateHub> _hub = hub;

        public Task<List<Equipment>> GetAllAsync()
        {
            return _repo.GetAllAsync();
        }

        public Task<Equipment?> GetByIdAsync(int id)
        {
            return _repo.GetByIdAsync(id);
        }

        public Task<List<StateChange>> GetHistoryAsync(int id)
        {
            return _repo.GetHistoryAsync(id);
        }

        public Task<List<ProductionOrder>> GetOrdersAsync(int id)
        {
            return _repo.GetOrdersAsync(id);
        }

        public Task<ProductionOrder?> GetCurrentOrderAsync(int id)
        {
            return _repo.GetCurrentOrderAsync(id);
        }
        public async Task<Equipment?> UpdateStateAsync(int equipmentId, ProductionState newState, string changedBy)
        {
            var eq = await _repo.GetByIdAsync(equipmentId);
            if (eq == null) return null;

            eq.CurrentState = newState;
            await _repo.AddStateChangeAsync(new StateChange
            {
                EquipmentId = equipmentId,
                NewState = newState,
                ChangedBy = changedBy ?? "unknown",
                Timestamp = DateTime.UtcNow
            });
            await _repo.SaveChangesAsync();

            var payload = new
            {
                equipmentId = eq.Id,
                newState = newState.ToString(),
                timestamp = DateTime.UtcNow,
                changedBy
            };

            await _hub.Clients.All.SendAsync("StateUpdated", payload);
            await _hub.Clients.Group($"equipment-{eq.Id}").SendAsync("StateUpdated", payload);

            return eq;
        }
    }
}
