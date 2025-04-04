using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;

namespace MessengerServer
{
    public class Server : IServer
    {
        private TcpListener listener;
        private static int port = 50000;
        private IPAddress ip;
        private Dictionary<Guid, Client> clients;
        private Dictionary<string, Room> rooms;

        private const string help = "List of commands:\n\t" +
                "/help shows this help\n\t" +
                "/disconnect disconnects user\n\t" +
                "/online shows users online in the chat\n\t" +
                "/name {username} set a new username";

        public Server(string ipAddress = "127.0.0.1", int port = 50000)
        {
            ip = IPAddress.Parse(ipAddress);
            listener = new TcpListener(ip, port);
            clients = new Dictionary<Guid, Client>();
            rooms = new Dictionary<string, Room>();
        }

        public async Task Run()
        {
            listener.Start();
            Console.WriteLine("Server started... ");

            while (true)
            {
                var tcpClient = await listener.AcceptTcpClientAsync();
                var client = new Client(tcpClient);
                clients.Add(client.Guid, client);
                Task.Run(async () => await ProcessClientAsync(client));
            }
        }

        private async Task ProcessClientAsync(Client client)
        {
            Console.WriteLine($"Connect {client.RemoteEndPoint}...");

            while (client.Connected)
            {
                try
                {
                    var message = await client.Reader.ReadLineAsync();
                    if (message == null)
                        continue;
                    if(message.StartsWith('/'))
                    {
                        var tokens = message.Split();
                        var command = tokens[0];
                        var arguments = tokens.Skip(1).ToArray();
                        await ProcessCommand(client, command, arguments);
                    }
                    else
                    {
                        await rooms[client.CurrentRoomTag].SendMessageBroadcastAsync(client, message);
                    }
                }
                catch(IOException io)
                {
                    Console.WriteLine($"User {client.Username} disconnected");
                    clients.Remove(client.Guid);
                    client.Disconnect();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                    break;
                }
            }
        }

        private async Task ProcessCommand(Client client, string command, string[] arguments)
        {
            if (command == "/name" && arguments.Length == 1)
            {
                await SetUsernameAsync(client, arguments[0]);
            }
            else if (command == "/disconnect")
            {
                await DisconnectUserAsync(client);
            }
            else if(command == "/help")
            {
                await client.SendMessageToClient(help);
            }
            else if (command == "/create" && arguments.Length <= 1)
            {
                var roomName = arguments.Length == 1 ? arguments[0] : string.Empty;
                await CreateRoomAsync(client, roomName);
            }
            else if (command == "/join" && arguments.Length == 1)
            {
                await JoinRoomAsync(client, arguments[0]);
            }
            else if (command == "/quit")
            {
                await QuitAsync(client);
            }
            else if (command == "/list")
            {
                await ListAsync(client);
            }
            else
            {
                await client.SendMessageToClient($"EXECUTION DENIED");
            }
        }

        private async Task ListAsync(Client client)
        {
            var message = rooms[client.CurrentRoomTag].ListUsers();
            await client.SendMessageToClient(message);
        }

        private async Task QuitAsync(Client client)
        {
            try
            {
                await rooms[client.CurrentRoomTag].RemoveClientFromRoom(client);
                client.CurrentRoomTag = string.Empty;
                await client.SendMessageToClient($"QIUT ACCEPTED");
            }
            catch
            {
                await client.SendMessageToClient($"QIUT DENIED");
                throw;
            }
        }

        private async Task JoinRoomAsync(Client client, string tag)
        {
            if (rooms.TryGetValue(tag, out var room))
            {
                await client.SendMessageToClient($"JOIN ACCEPTED {room.Tag}");
                room.Join(client);
                client.CurrentRoomTag = tag;
            }
            else
            {
                await client.SendMessageToClient($"JOIN DENIED {room.Tag}");
            }
        }

        private async Task CreateRoomAsync(Client client, string roomName)
        {
            var room = new Room(client, roomName);
            rooms.Add(room.Tag, room);
            await client.SendMessageToClient($"CREATE ACCEPTED {room.Tag}");
        }

        private async Task SetUsernameAsync(Client client, string name)
        {
            try
            {
                client.Username = name;
                await client.SendMessageToClient($"NAME ACCEPTED");
            }
            catch
            {
                await client.SendMessageToClient($"NAME DENIED");
            }
        }

        private async Task DisconnectUserAsync(Client client)
        {
            try
            {
                await client.SendMessageToClient("DISCONNECT ACCEPTED");
                clients.Remove(client.Guid);
                client.Disconnect();
            }
            catch
            {
                await client.SendMessageToClient("DISCONNECT DENIED");
                throw;
            }
        }
    }
}