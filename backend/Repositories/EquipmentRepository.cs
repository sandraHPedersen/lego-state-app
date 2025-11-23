using Microsoft.EntityFrameworkCore;
using RedYellowGreen.Api.Data.Models;
using RedYellowGreen.Api.Data.Models.Entities;
using RedYellowGreen.Api.Interfaces;

namespace RedYellowGreen.Api.Repositories
{
    public class EquipmentRepository(AppDbContext db) : IEquipmentRepository
    {
        private readonly AppDbContext _db = db;

        public Task<List<Equipment>> GetAllAsync()
        {
            return _db.Equipments.ToListAsync();
        }

        public Task<Equipment?> GetByIdAsync(int id)
        {
            return _db.Equipments.FindAsync(id).AsTask();
        }

        public Task<List<StateChange>> GetHistoryAsync(int equipmentId, int limit = 200)
        {
            return _db.StateChanges
               .Where(s => s.EquipmentId == equipmentId)
               .OrderByDescending(s => s.Timestamp)
               .Take(limit)
               .ToListAsync();
        }


        public Task<List<ProductionOrder>> GetOrdersAsync(int equipmentId)
        {
            return _db.ProductionOrders
               .Where(o => o.EquipmentId == equipmentId)
               .OrderBy(o => o.ScheduledStart)
               .ToListAsync();
        }

        public Task<ProductionOrder?> GetCurrentOrderAsync(int equipmentId)
        {
            var now = DateTime.UtcNow;
            return _db.ProductionOrders
                .Where(o => o.EquipmentId == equipmentId && o.Status == "InProgress")
                .OrderBy(o => o.ScheduledStart)
                .FirstOrDefaultAsync();
        }

        public Task AddStateChangeAsync(StateChange change)
        {
            _db.StateChanges.Add(change);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync()
        {
            return _db.SaveChangesAsync();
        }
    }
}
