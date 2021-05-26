using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using Teste_Rabbit.Configurations;
using Teste_Rabbit.Infra;
using Teste_Rabbit.Interfaces;
using Teste_Rabbit.Service;

namespace Teste_Rabbit.IoC
{
    public static class IoC
    {
        private const string RABBIT_CONSUMER = "RabbitConsumer";
        private const string RABBIT_PRODUCER = "RabbitProducer";

        public static IServiceCollection ConfigureContainer(this IServiceCollection services, IConfiguration configuration)
        {
            //se eu quiser incluir as injeções em outro arquivo, inserir a linha abaixo:
            //Data/Domain/InfraModule.Register(services, configuration)
            //criar a classe e o método Register(IServiceCollection services, IConfiguration configuration) com as injeções

            services.AddSingleton<IRabbitService, RabbitService>();
            services.AddSingleton<IConsumerRabbit, ConsumerRabbit>();
            services.AddSingleton(configuration.GetSection(RABBIT_CONSUMER).Get<RabbitConsumer>());
            services.AddSingleton<IProducerRabbit, ProducerRabbit>();
            services.AddSingleton(configuration.GetSection(RABBIT_PRODUCER).Get<RabbitProducer>());

            return services;
        }

        public static IServiceCollection AddConsumerRabit(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConnectionFactory>(_ =>
            new ConnectionFactory
            {
                DispatchConsumersAsync=true,
                HostName="localhost",
                Port=5678,
                UserName="guest",
                Password="guest",
                RequestedHeartbeat=new TimeSpan(0, 0, 60)
            });
            return services;
        }
    }
}
