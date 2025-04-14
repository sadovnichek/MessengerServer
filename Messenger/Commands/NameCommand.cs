using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MessengerServer.Commands
{
    public class NameCommand : ICommand
    {
        public string Name => "/name";

        public string Description => throw new NotImplementedException();

        public async Task Execute(Client client, Server server, params string[] args)
        {
            if (!ValidateArguments(args))
                throw new InvalidOperationException();
            var oldName = client.Username;
            client.Username = args[0];
            if (client.State == State.Joined)
            {
                server.TryGetRoom(client.CurrentRoomTag, out var clientRoom);
                await clientRoom.SendMessageBroadcastAsync($"User {oldName} is now {args[0]}");
            }
            else
            {
                await server.SendMessageToClient(client, $"NAME ACCEPTED");
            }
        }

        private bool ValidateArguments(string[] args)
        {
            return args.Length == 1;
        }
    }
}
