using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Dynamic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace Users.API.Hubs
{
    public class UserHub : Hub
    {
        private readonly static ConnectionMapping<string> _connections = new ConnectionMapping<string>();

        public override async Task OnConnectedAsync()
        {
            var user = Context.User.Claims.First(x => x.Type == "id").Value;
            var otherConnections = _connections.GetConnections(user).ToList();

            if (!_connections.GetConnections(user).Contains(Context.ConnectionId))
            {
                _connections.Add(user, Context.ConnectionId);
            }
            otherConnections.ForEach(async (c) =>
            {
                if (c != Context.ConnectionId)
                {
                    await Clients.Client(c).SendAsync("disconnect", user);
                    _connections.Remove(user, c);
                }
            });

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var user = Context.User.Claims.First(x => x.Type == "id").Value;
            _connections.Remove(user, Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }
    }
}
