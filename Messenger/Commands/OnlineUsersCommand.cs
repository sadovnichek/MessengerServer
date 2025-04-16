using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer.Commands
{
    public class OnlineUsersCommand : ICommand
    {
        public string Name => "/online";

        public string Description => throw new NotImplementedException();

        public async Task Execute(Client client, Server server, params string[] args)
        {
            if (!ValidateUserState(client))
                throw new InvalidOperationException();
            server.TryGetRoom(client.CurrentRoomTag, out var room);
            var message = "ONLINE " + room.GetOnlineUsers();
            await server.SendMessageToClient(client, message);
        }

        public bool ValidateUserState(Client client)
        {
            return client.State == State.Joined;
        }
    }
}
