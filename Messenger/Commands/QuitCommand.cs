using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer.Commands
{
    public class QuitCommand : ICommand
    {
        public string Name => "/quit";

        public string Description => throw new NotImplementedException();

        public async Task Execute(Client client, Server server, params string[] args)
        {
            if (!ValidateClientState(client))
                throw new InvalidOperationException();
            server.TryGetRoom(client.CurrentRoomTag, out var clientRoom);
            await clientRoom.RemoveClientFromRoom(client);
            client.SetStateOnQuit();
            await server.SendMessageToClient(client, $"QUIT ACCEPTED");
        }

        private bool ValidateClientState(Client client)
        {
            return client.State == State.Joined;
        }
    }
}
