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

        public string Name { get; }

        private List<Client> participants;

        public Room(Client author, string roomName = "") 
        {
            Id = Guid.NewGuid();
            Tag = Id.ToString().Substring(0, 6);
            Name = roomName == "" ? Tag : roomName;
            participants = new List<Client>();
        }

        public string ListUsers()
        {
            return string.Join("\n", participants.Select(p => p.Username));
        }

        public void Join(Client client)
        {
            participants.Add(client);
        }

        public async Task RemoveClientFromRoom(Client client)
        {
            participants.Remove(client);
            await SendMessageBroadcastAsync(client, $"{client.Username} left the room");
        }

        public async Task SendMessageBroadcastAsync(Client client, string message)
        {
            foreach(var p in participants)
            {
                if(p.Guid != client.Guid)
                {
                    await p.SendMessageToClient($"[{client.Username}]: {message}");
                }
            }
        }
    }
}
