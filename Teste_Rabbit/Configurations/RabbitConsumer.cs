namespace Teste_Rabbit.Configurations
{
    public class RabbitConsumer
    {
        public string HostName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
        public string QueueName { get; set; }
        public int Port { get; set; }
        public string DeadLetterQueue { get; set; }
        public string DeadLetterExchange { get; set; }
    }
}
