using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Teste_Rabbit.Interfaces;

namespace Teste_Rabbit
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IRabbitService _service;

        public Worker(ILogger<Worker> logger, IRabbitService service)
        {
            _logger = logger;
            _service = service;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //tudo o que está aqui dentro será executado repetidamente

                _service.Execute();

                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(50000, stoppingToken);
            }
        }
    }
}
