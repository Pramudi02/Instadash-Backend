using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Backend.Hubs
{
    public class ChatHub : Hub
    {
        public async Task JoinUserGroup(string userName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userName}");
        }

        public async Task LeaveUserGroup(string userName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userName}");
        }

        public async Task SendMessage(string sender, string receiver, string text)
        {
            var timestamp = DateTime.UtcNow;
            var message = new {
                sender,
                receiver,
                text,
                timestamp
            };
            
            // Send to both sender and receiver groups
            await Clients.Group($"User_{sender}").SendAsync("ReceiveMessage", message);
            await Clients.Group($"User_{receiver}").SendAsync("ReceiveMessage", message);
        }

        
    }
}