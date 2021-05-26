using System;
using System.Collections.Generic;
using System.Text;

namespace Teste_Rabbit.Models
{
    public class RabbitResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }
        public DateTime CreatedAt { get; set; }

        public RabbitResponse()
        {
            Number = new Random().Next(10, 20);
            CreatedAt = DateTime.Now;
        }
    }
}
