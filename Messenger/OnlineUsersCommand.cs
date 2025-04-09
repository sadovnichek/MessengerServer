using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer
{
    public class OnlineUsersCommand : ICommand
    {
        public string Name => "/online";

        public string Description => throw new NotImplementedException();

        public async Task Execute(Client client, Server server, params string[] args)
        {
            try
            {
                if (!server.TryGetRoom(client.CurrentRoomTag, out var room))
                    throw new InvalidOperationException();
                var message = room.GetOnlineUsers();
                await client.SendMessageToClient(message);
            }
            catch
            {
                await client.SendMessageToClient($"ONLINE DENIED");
                throw;
            }
        }
    }
}
