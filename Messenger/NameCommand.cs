using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MessengerServer
{
    public class NameCommand : ICommand
    {
        public string Name => "/name";

        public string Description => throw new NotImplementedException();

        public async Task Execute(Client client, Server server, params string[] args)
        {
            if (!Validate(args))
                throw new InvalidOperationException();
            try
            {
                var oldName = client.Username;
                client.Username = args[0];
                if (client.State == State.Joined)
                {
                    server.TryGetRoom(client.CurrentRoomTag, out var clientRoom);
                    await clientRoom.SendMessageBroadcastAsync($"User {oldName} is now {args[0]}");
                }
                else
                {
                    await client.SendMessageToClient($"NAME ACCEPTED");
                }
            }
            catch
            {
                await client.SendMessageToClient($"NAME DENIED");
                throw;
            }
        }

        private bool Validate(params string[] args)
        {
            return args.Length == 1;
        }
    }
}
