using Newtonsoft.Json; //instalar pelo NuGet
using RabbitMQ.Client; //instalar pelo NuGet
using System.Text;
using Teste_Rabbit.Configurations;
using Teste_Rabbit.Interfaces;
using Teste_Rabbit.Models;

namespace Teste_Rabbit.Infra
{
    public class ProducerRabbit : IProducerRabbit
    {
        private readonly string _exchangeName;
        private readonly string _queueNameA;
        private readonly string _queueNameB;
        private readonly string _routingKeyA;
        private readonly string _routingKeyB;
        private const int persistentDeliveryMode = 2;
        private readonly ConnectionFactory _connectionFactory;
        private readonly IBasicProperties _queueProperties;
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
        public ProducerRabbit(RabbitProducer rabbitConfig)
        {
            _exchangeName = rabbitConfig.ExchangeName;
            _queueNameA = rabbitConfig.QueueNameA;
            _queueNameB = rabbitConfig.QueueNameB;
            _routingKeyA = rabbitConfig.RoutingKeyA;
            _routingKeyB = rabbitConfig.RoutingKeyB;
            _connectionFactory = new ConnectionFactory
            {
                HostName = rabbitConfig.HostName,
                Port = rabbitConfig.Port,
                UserName = rabbitConfig.UserName,
                Password = rabbitConfig.Password,
                VirtualHost = rabbitConfig.VirtualHost,
            };
            ExchangeDeclare();
            _queueProperties = CreateQueueProperties();
            Channel.ConfirmSelect();
        }

        public void Publish(object message)
        {
            BasicPublish(message);
            Channel.WaitForConfirmsOrDie();
            Channel.WaitForConfirms();
        }

        private void BasicPublish(object data)
        {
            var routingKey = (data as RabbitResponse).Id < 50 ? _routingKeyA : _routingKeyB;
            var jsonObject = JsonConvert.SerializeObject(data);
            Channel.BasicPublish(_exchangeName, routingKey, _queueProperties, Encoding.UTF8.GetBytes(jsonObject));
        }

        private void ExchangeDeclare()
        {
            //é possível declarar apenas a exchange, apenas a fila ou os dois

            //para adicionar argumentos na exchange:            
            //var args = new Dictionary<string, object>();
            //args.Add("x-message-ttl", 28800000);
            //ttl = tempo de vida da mensagem (ela se apaga sozinha depois desse tempo)

            //declarar a fila que será usada (se não existir, cria)
            //como aqui usa routing key, para dividir as mensagens em filas diferentes, são criadas 2 filas
            Channel.QueueDeclare(
                        queue: _queueNameA,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

            Channel.QueueDeclare(
                        queue: _queueNameB,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

            if (!string.IsNullOrEmpty(_exchangeName))
            {
                //declarar a exchange que será usada (se não existir, cria)
                //exchange do tipo Direct utiliza a routing key para selecionar a fila para onde ela vai
                Channel.ExchangeDeclare(
                            exchange: _exchangeName,
                            type: ExchangeType.Direct, //Direct, Fanout...
                            durable: true,
                            autoDelete: false,
                            arguments: null); //ou arguments: args

                //para ligar a fila ao exchange criado
                Channel.QueueBind(queue: _queueNameA,
                      exchange: _exchangeName,
                      routingKey: _routingKeyA);

                Channel.QueueBind(queue: _queueNameB,
                      exchange: _exchangeName,
                      routingKey: _routingKeyB);
            }
        }

        private IBasicProperties CreateQueueProperties()
        {
            //é como o header da mensagem
            IBasicProperties queueProperties = Channel.CreateBasicProperties();
            queueProperties.ContentType = "application/json";
            queueProperties.DeliveryMode = persistentDeliveryMode;
            queueProperties.Persistent = true;
            return queueProperties;
        }
    }
}
