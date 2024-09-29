using System.Net.Sockets;
using System.Net;
using System.Text.Json;
using System.IO;
using System;

class Server
{
    private TcpListener listener;
    private bool isRunning;

    public Server(int port)
    {
        listener = new TcpListener(IPAddress.Any, port);
    }

    public void Start()
    {
        Console.WriteLine("JSON TCP Server starting...");
        listener.Start();
        isRunning = true;

        while (isRunning)
        {
            Console.WriteLine("Waiting for a connection...");
            TcpClient socket = listener.AcceptTcpClient();
            Console.WriteLine("Client connected!");

            HandleClient(socket);
        }
    }

    public void Stop()
    {
        isRunning = false;
        listener.Stop();
        Console.WriteLine("Server stopped.");
    }

    private void HandleClient(TcpClient socket)
    {
        NetworkStream ns = socket.GetStream();
        StreamReader reader = new StreamReader(ns);
        StreamWriter writer = new StreamWriter(ns) { AutoFlush = true };

        Random random = new Random();
        bool clientConnected = true;

        while (clientConnected)
        {
            try
            {
                string message = reader.ReadLine();
                if (string.IsNullOrEmpty(message))
                {
                    writer.WriteLine("{\"error\": \"Empty message received\"}");
                    continue;
                }

                try
                {
                    var jsonMessage = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(message);

                    if (jsonMessage != null && jsonMessage.ContainsKey("method") &&
                        jsonMessage.ContainsKey("Tal1") && jsonMessage.ContainsKey("Tal2"))
                    {
                        string method = jsonMessage["method"].GetString().ToLower();
                        int tal1 = jsonMessage["Tal1"].GetInt32();
                        int tal2 = jsonMessage["Tal2"].GetInt32();

                        int result = 0;
                        switch (method)
                        {
                            case "add":
                                result = tal1 + tal2;
                                break;
                            case "subtract":
                                result = tal1 - tal2;
                                break;
                            case "random":
                                result = random.Next(tal1, tal2);
                                break;
                            default:
                                writer.WriteLine("{\"error\": \"Unknown method\"}");
                                continue;
                        }

                        var response = new { result = result };
                        writer.WriteLine(JsonSerializer.Serialize(response));
                    }
                    else
                    {
                        writer.WriteLine("{\"error\": \"Invalid JSON format\"}");
                    }
                }
                catch (Exception ex)
                {
                    writer.WriteLine($"{{\"error\": \"{ex.Message}\"}}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                clientConnected = false;
            }
        }

        socket.Close();
    }
}
