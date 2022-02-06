using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Text.Json;

namespace Example
{

    public class Message
    {
        public string sourceSquare { get; set; }
        public string targetSquare { get; set; }

    }
    public class Chess : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            Message message =
                JsonSerializer.Deserialize<Message>(e.Data);

            Console.WriteLine($"sourceSquare: {message.sourceSquare}");
            Console.WriteLine($"targetSquare: {message.targetSquare}");

            var outData = new Message
            {
                sourceSquare = "bruh",
                targetSquare = "yo"
            };

            Send(JsonSerializer.Serialize(outData));

        }
        protected override void OnClose(CloseEventArgs e)
        {
        }

        protected override void OnOpen()
        {
            var message = new Message
            {
                sourceSquare = "bruh",
                targetSquare = "yo"
            };
            Console.WriteLine("Open");
            Send(JsonSerializer.Serialize(message));
        }
    }
}
