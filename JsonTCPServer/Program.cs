using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

public class Program
{
    private static void Main()
    {
        int port = 5000; 
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Server is listening on port {port}...");

        while (true)
        {
            TcpClient socket = listener.AcceptTcpClient();
            Console.WriteLine("Client connected.");
            HandleClient(socket);
        }
    }

    private static void HandleClient(TcpClient socket)
    {
        using (NetworkStream stream = socket.GetStream())
        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
        using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
        {
            try
            {
               
                string requestJson = reader.ReadLine();
                var request = JsonSerializer.Deserialize<Request>(requestJson);

                if (request == null || string.IsNullOrEmpty(request.Method))
                {
                    SendError(writer, "Invalid JSON format");
                    return;
                }

                int result = 0;
                switch (request.Method)
                {
                    case "Random":
                        result = new Random().Next(request.Tal1, request.Tal2 + 1);
                        break;
                    case "Add":
                        result = request.Tal1 + request.Tal2;
                        break;
                    case "Subtract":
                        result = request.Tal1 - request.Tal2;
                        break;
                    default:
                        SendError(writer, "Unknown method");
                        return;
                }

               
                var response = new { result };
                writer.WriteLine(JsonSerializer.Serialize(response));
                writer.Flush();
            }
            catch (Exception ex)
            {
                SendError(writer, $"Error: {ex.Message}");
            }
            finally
            {
                socket.Close();
            }
        }
    }

    private static void SendError(StreamWriter writer, string message)
    {
        var errorResponse = new { error = message };
        writer.WriteLine(JsonSerializer.Serialize(errorResponse));
        writer.Flush();
    }
}

public class Request
{
    public string Method { get; set; }
    public int Tal1 { get; set; }
    public int Tal2 { get; set; }
}