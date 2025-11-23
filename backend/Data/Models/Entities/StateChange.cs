using System.ComponentModel.DataAnnotations;

namespace RedYellowGreen.Api.Data.Models.Entities
{
    public class StateChange : IStateChange
    {
        [Key]
        public int Id { get; set; }
        public int EquipmentId { get; set; }
        public ProductionState NewState { get; set; }
        public string ChangedBy { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }
}
