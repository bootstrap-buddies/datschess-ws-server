using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Text.Json;
using static WebSocket.ChessEngineProcessServer;

namespace Example
{

    public class Message
    {
        public string sourceSquare { get; set; }
        public string targetSquare { get; set; }
        public string[] history { get; set; }
    }
    public class Chess : WebSocketBehavior
    {
        private WebSocket.ChessEngineProcessServer chessEngineProcessServer;

        protected override void OnMessage(MessageEventArgs e)
        {
            Message message =
                JsonSerializer.Deserialize<Message>(e.Data);

            Console.WriteLine($"sourceSquare: {message.sourceSquare}");
            Console.WriteLine($"targetSquare: {message.targetSquare}");

            String historyString = historyToString(message.history);
            Console.WriteLine($"history: {historyString}");

            MessageFromClientCallback callback = MessageFromClient;
            chessEngineProcessServer.sendMessage(message, historyString, callback);
        }

        private string historyToString(string[] history)
        {
            string historyString = "";
            for (int i = 0; i < history.Length; ++i)
            {
                if (i != 0)
                {
                    historyString += " ";
                }
                historyString += history[i];
            }

            return historyString;
        }

        // Callback function that receives a move to this class
        public void MessageFromClient(string move)
        {
            var outData = new Message
            {
                sourceSquare = move.Substring(0,2),
                targetSquare = move.Substring(2, 2)
            };
            Send(JsonSerializer.Serialize(outData));
        }

        protected override void OnClose(CloseEventArgs e)
        {
        }

        protected override void OnOpen()
        {
            chessEngineProcessServer = new WebSocket.ChessEngineProcessServer();
            Console.WriteLine("Open");
        }
    }
}
