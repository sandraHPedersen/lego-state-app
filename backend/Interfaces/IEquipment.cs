namespace RedYellowGreen.Api.Data.Models.Entities
{
    public interface IEquipment
    {
        int Id { get; set; }
        string Name { get; set; }
        ProductionState CurrentState { get; set; }
    }
}
