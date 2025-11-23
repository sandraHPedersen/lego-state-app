using System.ComponentModel.DataAnnotations;

namespace RedYellowGreen.Api.Data.Models.Entities
{
    public enum ProductionState { Red = 0, Yellow = 1, Green = 2 }

    public class Equipment : IEquipment
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ProductionState CurrentState { get; set; }
    }
}
