using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer
{
    public class Room
    {
        public Guid Id { get; }

        public string Tag { get; }

        public int CountOnline => participants.Count;

        private List<Client> participants;

        private Server server;

        public Room(Server server) 
        {
            Id = Guid.NewGuid();
            Tag = Id.ToString().Substring(0, 6);
            participants = new List<Client>();
            this.server = server;
        }

        public string GetOnlineUsers()
        {
            return string.Join(";", participants.Select(p => p.Username));
        }

        public async Task Join(Client client)
        {
            await SendMessageBroadcastAsync($"JOINED {client.Username}");
            participants.Add(client);
        }

        public async Task RemoveClientFromRoom(Client client)
        {
            participants.Remove(client);
            await SendMessageBroadcastAsync($"LEFT {client.Username}");
        }

        public async Task SendMessageFromClientAsync(Client client, string message)
        {
            foreach(var p in participants)
            {
                if(p.Guid != client.Guid)
                {
                    await server.SendMessageToClient(p, $"[{client.Username}]: {message}");
                }
            }
        }

        public async Task SendMessageBroadcastAsync(string message)
        {
            foreach (var p in participants)
            {
                await server.SendMessageToClient(p, message);
            }
        }
    }
}
