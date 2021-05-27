## Worker em .NET que conecta com RabbitMQ

Para estudos do Rabbit, foi criado um Worker em .NET Core que consome e publica no Rabbit.

Ele consome a mensagem abaixo na fila "fila.consumer" do localhost:

{

"Id": int,
  
  "Name": "string"
  
}


E publica a resposta abaixo na exchange "exchange.teste" do localhost:

{

  "Id":500, //o Id recebido
  
  "Name":"Alguem", //o nome recebido
  
  "Number":10, //um número aleatório gerado pelo sistema
  
  "CreatedAt":"2021-05-26T12:12:43.3048465-03:00" //a data e hora em que a mensagem foi publicada
  
}

