using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teste_Rabbit.Configurations;
using Teste_Rabbit.Interfaces;
using Teste_Rabbit.Models;

namespace Teste_Rabbit.Infra
{
    public class ConsumerRabbit : IConsumerRabbit
    {
        private readonly string _queueName;
        private readonly ConnectionFactory _connectionFactory;
        EventingBasicConsumer _consumer;
        private IModel _channel { get; set; }
        private IModel Channel
        {
            get
            {
                if (_channel is null || _channel.IsClosed)
                {
                    var connection = _connectionFactory.CreateConnection();
                    _channel = connection.CreateModel();
                }
                return _channel;
            }
        }

        public ConsumerRabbit(RabbitConsumer rabbitConfig)
        {
            _queueName = rabbitConfig.QueueName;
            _connectionFactory = new ConnectionFactory
            {
                HostName = rabbitConfig.HostName,
                Port = rabbitConfig.Port,
                UserName = rabbitConfig.UserName,
                Password = rabbitConfig.Password,
                VirtualHost = rabbitConfig.VirtualHost,
            };
            QueueDeclare();
        }

        public void GetMessage(Action<RabbitRequest, BasicDeliverEventArgs> dequeue)
        {
            //esse parâmetro Action é o método que será executado assim que pegar a mensagem

            _consumer = new EventingBasicConsumer(Channel);
            _consumer.Received += (model, ea) =>
            {
                //a mensaem primeiro é decodificada para string para só depois ser convertida na classe que será usada
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    var formatedMessage = System.Text.Json.JsonSerializer.Deserialize<RabbitRequest>(message);

                    //chama o método do Service passando a mensagem que recebeu
                    dequeue(formatedMessage, ea);

                }
                catch
                {
                    RetryMessage(ea);
                }
            };

            //para limitar o numero de mensagens lida por vez, usa o Qos
            //prefetchCount = número máximo de mensagens lida por vez (0 = sem limite, infinito)
            Channel.BasicQos(0, 1, false);

            Channel.BasicConsume(
                queue: _queueName, 
                autoAck: false,
                consumer: _consumer);
        }

        private void QueueDeclare()
        {
            //precisa linkar a fila com uma dead-letter, assim, se der erro na mensagem, ela é automaticamente
            // enviada para a fila dessa dead-letter, não é perdida nem fica sendo lida infinitamente
            var args = new Dictionary<string, object>();
            args.Add("x-dead-letter-exchange", "deadletter.exchangeteste");

            Channel.QueueDeclare(
                        queue: _queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: args);

            //precisa criar as dead-letters
            CreateDeadLetter();
        }

        private void CreateDeadLetter()
        {
            Channel.QueueDeclare(
                        queue: "deadletter.filateste",
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

            Channel.ExchangeDeclare(
                        exchange: "deadletter.exchangeteste",
                        type: ExchangeType.Fanout,
                        durable: true,
                        autoDelete: false,
                        arguments: null);

            //para ligar a fila com a exchange:
            Channel.QueueBind(
                "deadletter.filateste",
                "deadletter.exchangeteste",
                "",
                null);
        }

        public void RetryMessage(BasicDeliverEventArgs arg)
        {
            if(GetRetryCount(arg) <= 3)
            {
                //o Nack diz para o Rabbit que houve erro na leitura
                //o requeue true mantém na mesma fila, requeue false apaga da fila e manda para a dead-letter quando ela existe
                Channel.BasicNack(arg.DeliveryTag, false, false);
            }
            else
            {
                string eventArgs = JsonConvert.SerializeObject(arg, new JsonSerializerSettings
                {
                    Error = delegate (object sender, ErrorEventArgs args)
                    {
                        args.ErrorContext.Handled = true;
                    }
                });

                Console.WriteLine("Erro ao ler da fila! ", eventArgs);

                Channel.BasicAck(arg.DeliveryTag, false);
            }
        }

        private int GetRetryCount(BasicDeliverEventArgs arg)
        {
            var count = 0;

            if (arg.BasicProperties.Headers != null
                && arg.BasicProperties.Headers.ContainsKey("x-death")
                && arg.BasicProperties.Headers["x-death"] is List<object> xdeath
                && xdeath.FirstOrDefault() is Dictionary<string, object> headers)
            {
                count = Convert.ToInt32(headers["count"]);
            }

            return ++count;
        }

        public void ProcessFinishMessage(BasicDeliverEventArgs arg)
        {
            //o ack diz para o Rabbit que a mensagem foi lida e processada com sucesso, que pode apagar
            Channel.BasicAck(arg.DeliveryTag, false);
        }

    }
}
