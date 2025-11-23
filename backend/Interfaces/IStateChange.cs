namespace RedYellowGreen.Api.Data.Models.Entities
{
    public interface IStateChange
    {
        int Id { get; set; }
        int EquipmentId { get; set; }
        ProductionState NewState { get; set; }
        string ChangedBy { get; set; }
        DateTime Timestamp { get; set; }
    }
}
