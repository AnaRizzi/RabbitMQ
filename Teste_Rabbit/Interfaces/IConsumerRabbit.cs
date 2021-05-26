using RabbitMQ.Client.Events;
using System;
using Teste_Rabbit.Models;

namespace Teste_Rabbit.Interfaces
{
    public interface IConsumerRabbit
    {
        void GetMessage(Action<RabbitRequest, BasicDeliverEventArgs> dequeue);
        void ProcessFinishMessage(BasicDeliverEventArgs arg);
        void RetryMessage(BasicDeliverEventArgs arg);
    }
}
