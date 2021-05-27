namespace Teste_Rabbit.Configurations
{
    public class RabbitProducer
    {
        public string HostName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
        public string ExchangeName { get; set; }
        public int Port { get; set; }
        public string RoutingKeyA { get; set; }
        public string RoutingKeyB { get; set; }
        public string QueueNameA { get; set; }
        public string QueueNameB { get; set; }
    }
}