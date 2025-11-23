using Microsoft.AspNetCore.SignalR;

namespace RedYellowGreen.Api.Hubs
{
    public class StateHub : Hub
    {
        // Client calls this to subscribe to updates for a specific equipment id
        public Task Subscribe(int equipmentId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, GroupName(equipmentId));
        }

        // Client calls this to unsubscribe from updates for a specific equipment id
        public Task Unsubscribe(int equipmentId)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(equipmentId));
        }

        private static string GroupName(int equipmentId) => $"equipment-{equipmentId}";
    }
}
