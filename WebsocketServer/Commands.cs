using System.Net.WebSockets;

namespace WebsocketServer;

public class ServerCommandInterpreter
{
    public Dictionary<string, IServerCommand> commands;

    public ServerCommandInterpreter()
    {
        commands = new Dictionary<string, IServerCommand>
        {
            { "Ping", new PingCommand() },
            { "Register", new RegisterCommand() },
            { "Login", new LoginCommand() },
            { "Message", new MessageCommand() }
        };
    }
}
public interface IServerCommand
{
    void Execute(WebSocket socket, dynamic message);
}

public class PingCommand : IServerCommand
{
    public void Execute(WebSocket socket, dynamic message)
    {
        // Implement your logic here
        Console.WriteLine("PING");
        Console.WriteLine(message.data);
    }
}

public class RegisterCommand : IServerCommand
{
    public void Execute(WebSocket socket, dynamic message)
    {
        // Implement your logic here
    }
}

public class LoginCommand : IServerCommand
{
    public void Execute(WebSocket socket, dynamic message)
    {
        // Implement your logic here
    }
}

public class MessageCommand : IServerCommand
{
    public void Execute(WebSocket socket, dynamic message)
    {
        // Implement your logic here
    }
}