using System.ComponentModel.DataAnnotations;

namespace RedYellowGreen.Api.Data.Models.Entities
{
    public class ProductionOrder : IProductionOrder
    {
        [Key]
        public int Id { get; set; }
        public string OrderNumber { get; set; } = null!;
        public int? EquipmentId { get; set; }
        public DateTime ScheduledStart { get; set; }
        public DateTime? ScheduledEnd { get; set; }
        public string Status { get; set; } = "Scheduled";
        public string? Description { get; set; }
    }
}
