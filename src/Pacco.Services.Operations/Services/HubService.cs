using System.Threading.Tasks;
using Pacco.Services.Operations.DTO;

namespace Pacco.Services.Operations.Services
{
    public class HubService : IHubService
    {
        private readonly IHubWrapper _hubContextWrapper;

        public HubService(IHubWrapper hubContextWrapper)
        {
            _hubContextWrapper = hubContextWrapper;
        }

        public async Task PublishOperationPendingAsync(OperationDto operation)
            => await _hubContextWrapper.PublishToUserAsync(operation.UserId,
                "operation_pending",
                new
                {
                    id = operation.Id,
                    name = operation.Name,
                    resource = operation.Resource
                }
            );

        public async Task PublishOperationCompletedAsync(OperationDto operation)
            => await _hubContextWrapper.PublishToUserAsync(operation.UserId,
                "operation_completed",
                new
                {
                    id = operation.Id,
                    name = operation.Name,
                    resource = operation.Resource
                }
            );

        public async Task PublishOperationRejectedAsync(OperationDto operation)
            => await _hubContextWrapper.PublishToUserAsync(operation.UserId,
                "operation_rejected",
                new
                {
                    id = operation.Id,
                    name = operation.Name,
                    resource = operation.Resource,
                    code = operation.Code,
                    reason = operation.Resource
                }
            );
    }
}