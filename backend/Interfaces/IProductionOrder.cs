namespace RedYellowGreen.Api.Data.Models.Entities
{
    public interface IProductionOrder
    {
        int Id { get; set; }
        string OrderNumber { get; set; }
        int? EquipmentId { get; set; }
        DateTime ScheduledStart { get; set; }
        DateTime? ScheduledEnd { get; set; }
        string Status { get; set; }
        string? Description { get; set; }
    }
}
