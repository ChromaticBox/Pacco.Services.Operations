using System;
using Convey;
using Convey.Auth;
using Convey.Configurations.Vault;
using Convey.CQRS.Commands;
using Convey.CQRS.Events;
using Convey.CQRS.Queries;
using Convey.Discovery.Consul;
using Convey.HTTP;
using Convey.LoadBalancing.Fabio;
using Convey.MessageBrokers.RabbitMQ;
using Convey.Metrics.AppMetrics;
using Convey.Persistence.MongoDB;
using Convey.Persistence.Redis;
using Convey.Tracing.Jaeger;
using Convey.Tracing.Jaeger.RabbitMQ;
using Convey.WebApi;
using Convey.WebApi.CQRS;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Pacco.Services.Operations.Api.Handlers;
using Pacco.Services.Operations.Api.Hubs;
using Pacco.Services.Operations.Api.Services;
using Pacco.Services.Operations.Api.Types;

namespace Pacco.Services.Operations.Api.Infrastructure
{
    public static class Extensions
    {
        public static string ToUserGroup(this Guid userId) => $"users:{userId}";

        public static IConveyBuilder AddInfrastructure(this IConveyBuilder builder)
        {
            builder.Services.AddTransient<ICommandHandler<ICommand>, GenericCommandHandler<ICommand>>()
                .AddTransient<IEventHandler<IEvent>, GenericEventHandler<IEvent>>()
                .AddTransient<IEventHandler<IRejectedEvent>, GenericRejectedEventHandler<IRejectedEvent>>()
                .AddTransient<IHubService, HubService>()
                .AddTransient<IHubWrapper, HubWrapper>()
                .AddTransient<IOperationsService, OperationsService>();

            return builder.AddJwt()
                .AddCommandHandlers()
                .AddEventHandlers()
                .AddQueryHandlers()
                .AddHttpClient()
                .AddConsul()
                .AddFabio()
                .AddRabbitMq(plugins: p => p.RegisterJaeger())
                .AddMongo()
                .AddMetrics()
                .AddJaeger()
                .AddVault()
                .AddRedis()
                .AddSignalR();
        }

        public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
        {
            app.UseErrorHandler()
                .UseVault()
                .UseJaeger()
                .UseInitializers()
                .UseConsul()
                .UseMetrics()
                .UseStaticFiles()
                .UseSignalR(r => r.MapHub<PaccoHub>("/pacco"))
                .UseRabbitMq()
                .SubscribeMessages();

            return app;
        }

        private static IConveyBuilder AddSignalR(this IConveyBuilder builder)
        {
            var options = builder.GetOptions<SignalrOptions>("signalR");
            builder.Services.AddSingleton(options);
            var signalR = builder.Services.AddSignalR();
            if (!options.Backplane.Equals("redis", StringComparison.InvariantCultureIgnoreCase))
            {
                return builder;
            }

            var redisOptions = builder.GetOptions<RedisOptions>("redis");
            signalR.AddRedis(redisOptions.ConnectionString);

            return builder;
        }
    }
}