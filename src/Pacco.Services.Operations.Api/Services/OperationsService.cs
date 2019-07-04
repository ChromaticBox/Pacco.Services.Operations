using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Pacco.Services.Operations.Api.DTO;
using Pacco.Services.Operations.Api.Types;

namespace Pacco.Services.Operations.Api.Services
{
    public class OperationsService : IOperationsService
    {
        private readonly IDistributedCache _cache;

        public OperationsService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<(bool, OperationDto)> TrySetAsync(Guid id, Guid userId, string name, OperationState state,
            string resource, string code = null, string reason = null)
        {
            var operation = await GetAsync(id);
            if (operation is null)
            {
                operation = new OperationDto();
            }
            else if (operation.State == OperationState.Completed || operation.State == OperationState.Rejected)
            {
                return (false, operation);
            }

            operation.Id = id;
            operation.UserId = userId;
            operation.Name = name;
            operation.State = state;
            operation.Resource = resource;
            operation.Code = code ?? string.Empty;
            operation.Reason = reason ?? string.Empty;
            await _cache.SetStringAsync(id.ToString("N"),
                JsonConvert.SerializeObject(operation),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                });

            return (true, operation);
        }

        public async Task<OperationDto> GetAsync(Guid id)
        {
            var operation = await _cache.GetStringAsync(id.ToString("N"));

            return string.IsNullOrWhiteSpace(operation) ? null : JsonConvert.DeserializeObject<OperationDto>(operation);
        }
    }
}