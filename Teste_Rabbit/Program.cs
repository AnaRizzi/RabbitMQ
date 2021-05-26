using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Teste_Rabbit.IoC;

namespace Teste_Rabbit
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    //cria a linha abaixo para incluir a injeção de dependência e add o arquivo IoC.cs e o using
                    services.ConfigureContainer(hostContext.Configuration);
                    services.AddHostedService<Worker>();
                });
    }
}
