using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Pacco.Services.Operations.Hubs;

namespace Pacco.Services.Operations.Services
{
    public class HubWrapper : IHubWrapper
    {
        private readonly IHubContext<PaccoHub> _hubContext;

        public HubWrapper(IHubContext<PaccoHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task PublishToUserAsync(Guid userId, string message, object data)
            => await _hubContext.Clients.Group(userId.ToUserGroup()).SendAsync(message, data);

        public async Task PublishToAllAsync(string message, object data)
            => await _hubContext.Clients.All.SendAsync(message, data);
    }
}