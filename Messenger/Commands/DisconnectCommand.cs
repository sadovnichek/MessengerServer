using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer.Commands
{
    public class DisconnectCommand : ICommand
    {
        public string Name => "/disconnect";

        public string Description => throw new NotImplementedException();

        public async Task Execute(Client client, Server server, params string[] args)
        {
            await server.SendMessageToClient(client, "DISCONNECT ACCEPTED");
            client.Disconnect();
        }
    }
}
