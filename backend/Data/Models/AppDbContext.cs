using Microsoft.EntityFrameworkCore;
using RedYellowGreen.Api.Data.Models.Entities;

namespace RedYellowGreen.Api.Data.Models
{
    public class AppDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Equipment> Equipments { get; set; } = null!;
        public DbSet<StateChange> StateChanges { get; set; } = null!;
        public DbSet<ProductionOrder> ProductionOrders { get; set; } = null!;
    }
}
