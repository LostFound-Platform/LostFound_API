using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SignalRLayer
{
    public class SystemHub : Hub
    {
        public static Dictionary<string, string> UserConnections = new();

        public override async Task OnConnectedAsync()
        {
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value; // Get role from user signed in

            if (role == "Admin")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admin"); // Group admin
            }

            var userEmail = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userEmail != null)
            {
                UserConnections[userEmail] = Context.ConnectionId;
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userEmail = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userEmail != null)
            {
                UserConnections.Remove(userEmail);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
