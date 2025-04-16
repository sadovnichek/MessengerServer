using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer.Commands
{
    public class MyselfCommand : ICommand
    {
        public string Name => "/myself";

        public string Description => throw new NotImplementedException();

        public async Task Execute(Client client, Server server, params string[] args)
        {
            await server.SendMessageToClient(client, $"YOU ARE {client.Username}");
        }
    }
}
