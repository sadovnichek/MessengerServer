using System.Net.Sockets;
using System.Net;

namespace MessengerServer
{
    public class Server : IServer
    {
        private TcpListener listener;
        private Dictionary<Guid, Client> clients;
        private Dictionary<string, Room> rooms;
        private List<ICommand> commands;
        private Dictionary<string, ICommand> helper;

        public Server()
        {
            var (ipAddress, port) = GetIPAndPort();
            listener = new TcpListener(IPAddress.Parse(ipAddress), int.Parse(port));
            clients = new Dictionary<Guid, Client>();
            rooms = new Dictionary<string, Room>();

            commands = new List<ICommand>()
            {
                new DisconnectCommand(),
                new JoinCommand(),
                new NameCommand(),
                new OnlineUsersCommand(),
                new QuitCommand(),
                new RoomCommand()
            };

            helper = commands.ToDictionary(c => c.Name, c => c);
        }

        public async Task Run()
        {
            listener.Start();
            Console.WriteLine("Server started");

            while (!Console.KeyAvailable)
            {
                var tcpClient = await listener.AcceptTcpClientAsync();
                var client = new Client(tcpClient);
                clients.Add(client.Guid, client);
                Task.Run(async () => await ProcessClientAsync(client));
            }
        }

        private async Task ProcessClientAsync(Client client)
        {
            Console.WriteLine($"Connected {client.RemoteEndPoint}");
            while (client.Connected)
            {
                try
                {
                    var message = await client.Reader.ReadLineAsync();
                    if (message == null)
                        continue;
                    await HandleClientMessage(client, message);
                }
                catch (IOException)
                {
                    Console.WriteLine($"User {client.Username} suddenly disconnected");
                    clients.Remove(client.Guid);
                    client.Disconnect();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private async Task HandleClientMessage(Client client, string message)
        {
            if (message.StartsWith('/'))
            {
                var tokens = message.Split();
                var command = tokens[0];
                var arguments = tokens.Skip(1).ToArray();
                await ProcessCommand(client, command, arguments);
            }
            else
            {
                if (client.State == State.Joined)
                {
                    var room = rooms[client.CurrentRoomTag];
                    await room.SendMessageFromClientAsync(client, message);
                }
                else
                    await client.SendMessageToClient("YOU HAVE NOT JOINED ANY ROOM");
            }
        }

        private async Task ProcessCommand(Client client, string command, string[] arguments)
        {
            try
            {
                await helper[command].Execute(client, this, arguments);
            }
            catch 
            {
                await client.SendMessageToClient($"EXECUTION DENIED");
                throw;
            }
        }

        public string CreateRoom()
        {
            var room = new Room();
            rooms.Add(room.Tag, room);
            return room.Tag;
        }

        public bool TryGetRoom(string tag, out Room room)
        {
            return rooms.TryGetValue(tag, out room);
        }

        public void RemoveClient(Guid clientGuid)
        {
            clients.Remove(clientGuid);
        }

        private (string, string) GetIPAndPort()
        {
            var config = File.ReadLines("./config.txt").ToArray()[0].Split(':');
            return (config[0], config[1]);
        }
    }
}