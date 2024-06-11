using System.Net;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;

namespace WebsocketServer;

public class Server
{
    public readonly List<WebSocket> Clients = new ();
    public static bool IsRunning { get; private set; }
    public static string RootDirectory { get; } = $@"{AppDomain.CurrentDomain.BaseDirectory}..\..\..\";
    public static string StaticDirectory { get; }  = Path.Combine(RootDirectory, "wwwroot");

    public event Action<WebSocket> OnWebsocketConnect;
    public event Action<WebSocket, dynamic> OnWebsocketMessage;
    public event Action<WebSocket> OnWebsocketClosed;
    
    private readonly ServerCommandInterpreter _interpreter = new();
    private HttpListener _listener = null!; //(null!) don't want this var in the constructor so promise compiler we are good boys

    public Server()
    {
        OnWebsocketConnect += (socket) =>
        {
            Console.WriteLine("New WebSocket connection established.");
            Clients.Add(socket);
        };
        OnWebsocketMessage += (socket, message) =>
        {
            var cmd = (string)message?.cmd!;
            if (string.IsNullOrEmpty(cmd)) return;
            
            Console.WriteLine("Command Received: " + cmd);
            if (_interpreter.commands.TryGetValue(cmd, out var command))
            {
                try
                {
                    command.Execute(socket, message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            else
            {
                // Handle the case where the command is not found
                Console.WriteLine($"Command {cmd} not found :(");
            }
        };
        OnWebsocketClosed += (socket) =>
        {
            Console.WriteLine($"Server socket has been CLOSED");
            Clients.Remove(socket);
        };
    }

    public async Task StartServer(string address)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add(address);
        _listener.Start();
        IsRunning = true;

        Console.WriteLine("======================================");
        Console.WriteLine("=  Arrogant Websocket Server Online  =");
        Console.WriteLine("======================================");

        Console.WriteLine($"Listening at {address}...");

        while (IsRunning)
        {
            HttpListenerContext listenerContext = null;
            try
            {
                listenerContext = await _listener.GetContextAsync();
            }
            catch (HttpListenerException ex)
            {
                if (ex.ErrorCode == 995) return;
            }
            if(listenerContext == null) continue;
        
            if (listenerContext.Request.IsWebSocketRequest)
            {
                HttpListenerWebSocketContext webSocketContext = await listenerContext.AcceptWebSocketAsync(null);
                WebSocket webSocket = webSocketContext.WebSocket;

                OnWebsocketConnect.Invoke(webSocket);
                
                //do not await task so that they can run concurrently (accept multiple websocket connections)
                Task.Run(() => HandleClient(webSocket));
            }
            else
            {
                // Serve static files
                var filePath = StaticDirectory + $@"\index.html";

                if (File.Exists(filePath))
                {
                    var buffer = await File.ReadAllBytesAsync(filePath);
                    listenerContext.Response.ContentLength64 = buffer.Length;
                    listenerContext.Response.OutputStream.Write(buffer);
                    listenerContext.Response.OutputStream.Close();
                }
                else
                {
                    listenerContext.Response.StatusCode = 404;
                }
                listenerContext.Response.Close();
            }
        }
    }
    public void StopServer()
    {
        IsRunning = false;
        _listener.Stop();
    }
    private async Task HandleClient(WebSocket socket)
    {
        var buffer = new byte[1024 * 4];
        
        while (true)
        {
            if (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var messageJson = JsonConvert.DeserializeObject<dynamic>(message);
                
                OnWebsocketMessage.Invoke(socket, messageJson);
            }
            else if (socket.State == WebSocketState.CloseReceived)
            {
                OnWebsocketClosed.Invoke(socket);
                socket.Dispose();
                break;
            }
        }
    }
}
