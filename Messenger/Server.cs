using System.Net.Sockets;
using System.Net;
using MessengerServer.Commands;

namespace MessengerServer
{
    public class Server : IServer
    {
        private TcpListener listener;
        private Dictionary<string, Room> rooms;
        private Dictionary<string, ICommand> helper;

        public Server()
        {
            var (ipAddress, port) = GetIPAndPort();
            listener = new TcpListener(IPAddress.Parse(ipAddress), int.Parse(port));
            rooms = new Dictionary<string, Room>();
            helper = CommandHelper.GetCommands().ToDictionary(command => command.Name, command => command);
        }

        public async Task Run()
        {
            listener.Start();
            Log("Server started");

            while (true)
            {
                var tcpClient = await listener.AcceptTcpClientAsync();
                var client = new Client(tcpClient);
                Task.Run(async () => await ProcessClientAsync(client));
            }
        }

        private async Task ProcessClientAsync(Client client)
        {
            Log($"Connected {client.RemoteEndPoint}");
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
                    Log($"User {client.Username} was suddenly disconnected");
                    client.Disconnect();
                }
                catch (Exception e)
                {
                    Log(e.Message);
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
                    await SendMessageToClient(client, "YOU HAVE NOT JOINED ANY ROOM");
            }
        }

        private async Task ProcessCommand(Client client, string command, string[] arguments)
        {
            try
            {
                if (!helper.TryGetValue(command, out var instance))
                    await SendMessageToClient(client, $"EXECUTION DENIED. COMMAND {command} DOES NOT EXIST");
                else
                    await instance.Execute(client, this, arguments);
            }
            catch (InvalidOperationException)
            {
                await SendMessageToClient(client, $"EXECUTION DENIED. WRONG ARGUMENTS");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string CreateRoom()
        {
            var room = new Room(this);
            rooms.Add(room.Tag, room);
            return room.Tag;
        }

        public bool TryGetRoom(string tag, out Room room)
        {
            return rooms.TryGetValue(tag, out room);
        }

        public async Task SendMessageToClient(Client client, string message)
        {
            await client.Writer.WriteLineAsync(message);
            await client.Writer.FlushAsync();
        }

        public void Log(string message)
        {
            Console.WriteLine(message);
        }

        private (string, string) GetIPAndPort()
        {
            var config = File.ReadLines("./config.txt").ToArray()[0].Split(':');
            return (config[0], config[1]);
        }
    }
}