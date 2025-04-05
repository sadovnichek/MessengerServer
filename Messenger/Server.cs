using System.Net.Sockets;
using System.Net;

namespace MessengerServer
{
    public class Server : IServer
    {
        private TcpListener listener;
        private IPAddress ip;
        private Dictionary<Guid, Client> clients;
        private Dictionary<string, Room> rooms;

        public Server()
        {
            var (ipAddress, port) = GetIPAndPort();
            ip = IPAddress.Parse(ipAddress);
            listener = new TcpListener(ip, int.Parse(port));
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
                        await rooms[client.CurrentRoomTag].SendMessageFromClientAsync(client, message);
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
            else if (command == "/room" && arguments.Length <= 1)
            {
                await CreateRoomAsync(client);
            }
            else if (command == "/join" && arguments.Length == 1)
            {
                await JoinRoomAsync(client, arguments[0]);
            }
            else if (command == "/quit")
            {
                await QuitAsync(client);
            }
            else if (command == "/online")
            {
                await ListOnlineAsync(client);
            }
            else
            {
                await client.SendMessageToClient($"EXECUTION DENIED");
            }
        }

        private async Task ListOnlineAsync(Client client)
        {
            var message = rooms[client.CurrentRoomTag].GetOnlineUsers();
            await client.SendMessageToClient(message);
        }

        private async Task QuitAsync(Client client)
        {
            try
            {
                var clientRoom = rooms[client.CurrentRoomTag];
                await clientRoom.RemoveClientFromRoom(client);
                if (clientRoom.CountOnline == 0)
                    rooms.Remove(client.CurrentRoomTag);
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
                await room.Join(client);
                await client.SendMessageToClient($"JOIN ACCEPTED {room.Tag}");
                client.CurrentRoomTag = tag;
            }
            else
            {
                await client.SendMessageToClient($"JOIN DENIED");
            }
        }

        private async Task CreateRoomAsync(Client client)
        {
            var room = new Room();
            rooms.Add(room.Tag, room);
            await client.SendMessageToClient($"CREATE ACCEPTED {room.Tag}");
        }

        private async Task SetUsernameAsync(Client client, string name)
        {
            try
            {
                var oldName = client.Username;
                client.Username = name;
                await client.SendMessageToClient($"NAME ACCEPTED");
                if(client.CurrentRoomTag != null)
                {
                    await rooms[client.CurrentRoomTag].SendMessageBroadcastAsync($"User {oldName} is now {name}");
                }
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

        private (string, string) GetIPAndPort()
        {
            var config = File.ReadLines("./config.txt").ToArray()[0].Split(':');
            return (config[0], config[1]);
        }
    }
}