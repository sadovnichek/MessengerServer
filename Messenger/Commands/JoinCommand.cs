using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer.Commands
{
    public class JoinCommand : ICommand
    {
        public string Name => "/join";

        public string Description => throw new NotImplementedException();

        public async Task Execute(Client client, Server server, params string[] args)
        {
            if (!ValidateArguments(args) || !ValidateClientState(client))
                throw new InvalidOperationException();
            var roomTag = args[0];
            if (server.TryGetRoom(roomTag, out var room))
            {
                await room.Join(client);
                client.SetStateOnJoin(roomTag);
                await server.SendMessageToClient(client, $"JOIN ACCEPTED {room.Tag}");
            }
            else
            {
                await server.SendMessageToClient(client, $"JOIN DENIED. ROOM {roomTag} DOES NOT EXIST");
            }
        }

        private bool ValidateArguments(string[] args)
        {
            return args.Length == 1;
        }

        private bool ValidateClientState(Client client)
        {
            return client.State == State.Connected;
        }
    }
}
