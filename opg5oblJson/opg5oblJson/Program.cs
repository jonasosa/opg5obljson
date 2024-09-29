using System.Net.Sockets;
using System.Text.Json;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Client");

        bool keepSending = true;

        TcpClient socket = new TcpClient("127.0.0.1", 7);
        NetworkStream ns = socket.GetStream();
        StreamReader reader = new StreamReader(ns);
        StreamWriter writer = new StreamWriter(ns);
        writer.AutoFlush = true;  // fandte en metode så jeg ikke skulle lave en writer.Flush(); være gang jeg ville sennde en besked

        while (keepSending)
        {
            Console.Write("Enter method (add, subtract, random or stop): ");
            string method = Console.ReadLine().ToLower();

            if (method == "stop")
            {
                keepSending = false;
                continue;
            }

            Console.Write("Enter first number: ");
            int tal1 = int.Parse(Console.ReadLine());

            Console.Write("Enter second number: ");
            int tal2 = int.Parse(Console.ReadLine());

            // Konstruér JSON objektet
            var message = new
            {
                method = method,
                Tal1 = tal1,
                Tal2 = tal2
            };

            // Serialiser til JSON og send
            string jsonMessage = JsonSerializer.Serialize(message);
            writer.WriteLine(jsonMessage);

            // Læs svaret fra serveren
            string response = reader.ReadLine();

            try
            {
                // Deserialize sever rasponse så cliented ikke ser noget json
                var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(response);

                // Hvis der er en result værdi, udskriv den
                if (jsonResponse.ContainsKey("result"))
                {
                    int result = jsonResponse["result"].GetInt32();
                    Console.WriteLine($"Response from server: {result}");
                }
                // Hvis der er en result error, udskriv den
                else if (jsonResponse.ContainsKey("error"))
                {
                    string error = jsonResponse["error"].GetString();
                    Console.WriteLine($"Error from server: {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse server response: {ex.Message}");
            }
            if (method.ToLower() == "stop")
            {
                keepSending = false;
            }
        }

        socket.Close();
    }
}
