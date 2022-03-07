using Example;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;

namespace WebSocket
{
    class ChessEngineProcessServer
    {
        public delegate void MessageFromClientCallback(string move);
        public void sendMessage(Message aMessage, String history, MessageFromClientCallback callback)
        {
            String msg = aMessage.sourceSquare + aMessage.targetSquare;
            Console.WriteLine("[SERVER]: Lets play chess!");

            var result = new List<string>();

            // Create separate process
            var anotherProcess = new Process
            {
                StartInfo =
                {
                    FileName = "C:/Users/ajdel/Dev/SeniorDesign/datschess-engine1/ConsoleApp1/ConsoleApp1/bin/Debug/net5.0/ConsoleApp1.exe",
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };

            // Create 2 anonymous pipes (read and write) for duplex communications (each pipe is one-way)
            using (var pipeRead = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable))
            using (var pipeWrite = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable))
            {
                // Pass to the other process handles to the 2 pipes
                anotherProcess.StartInfo.Arguments = pipeRead.GetClientHandleAsString() + " " + pipeWrite.GetClientHandleAsString();
                anotherProcess.Start();

                Console.WriteLine("[SERVER]: Started Engine...");
                Console.WriteLine();

                pipeRead.DisposeLocalCopyOfClientHandle();
                pipeWrite.DisposeLocalCopyOfClientHandle();

                try
                {
                    using (var sw = new StreamWriter(pipeWrite))
                    {
                        // Send a 'sync message' and wait for the other process to receive it
                        sw.WriteLine("SYNC");
                        pipeWrite.WaitForPipeDrain();
                        string message = "position startpos moves " + history;
                        Console.WriteLine("[SERVER]: Sending message to Engine: " + message);

                        // Send message to the other process
                        sw.WriteLine(message);
                        sw.WriteLine("go");
                        sw.WriteLine("END");
                    }

                    // Get message from the other process
                    using (var sr = new StreamReader(pipeRead))
                    {
                        string temp;

                        // Wait for 'sync message' from the other process
                        do
                        {
                            temp = sr.ReadLine();
                        } while (temp == null || !temp.StartsWith("SYNC"));

                        // Read until 'end message' from the other process
                        while ((temp = sr.ReadLine()) != null)
                        {
                            Console.WriteLine("[SERVER]: " + temp);
                            result.Add(temp);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //TODO Exception handling/logging
                    throw;
                }
                finally
                {
                    anotherProcess.WaitForExit();
                    anotherProcess.Close();
                }

                if (result.Count > 0)
                {
                    Console.WriteLine("[SERVER]: Received best move from engine: " + result[0]);
                    callback(result[0]);
                }

                Console.WriteLine("End of program");

            }
        }
    }
}
