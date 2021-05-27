using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using Teste_Rabbit.Interfaces;
using Teste_Rabbit.Models;

namespace Teste_Rabbit.Service
{
    public class RabbitService : IRabbitService
    {
        private readonly IConsumerRabbit _consumer;
        private readonly IProducerRabbit _producer;
        private readonly ILogger<RabbitService> _logger;

        public RabbitService(ILogger<RabbitService> logger, IConsumerRabbit consumer, IProducerRabbit producer)
        {
            _logger = logger;
            _consumer = consumer;
            _producer = producer;
        }
        public void Execute()
        {
            //vai no Consumer para pegar a mensagem e passa a próxima ação como parâmetro para ser chamada do próprio consumer
            _consumer.GetMessage(ProcessMessage);
        }

        public void ProcessMessage(RabbitRequest message, BasicDeliverEventArgs arg)
        {
            //é chamado pelo consumer, que já passa a mensagem
            //(se der erro, é tratado pelo catch de dentro do Consumer)
            _logger.LogInformation("Processando a mensagem de " + message.Name);

            var response = new RabbitResponse()
            {
                Id = message.Id,
                Name = message.Name
            };

            _producer.Publish(response);

            //precisa avisar o rabbit de que a mensagem recebida foi finalizada, para que possa sair da fila
            _consumer.ProcessFinishMessage(arg);

            _logger.LogInformation("Processo finalizado, mensagem publicada", response);
        }
    }
}
