using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer
{
    public class DisconnectCommand : ICommand
    {
        public string Name => "/disconnect";

        public string Description => throw new NotImplementedException();

        public async Task Execute(Client client, Server server, params string[] args)
        {
            try
            {
                await client.SendMessageToClient("DISCONNECT ACCEPTED");
                server.RemoveClient(client.Guid);
                client.Disconnect();
            }
            catch
            {
                await client.SendMessageToClient("DISCONNECT DENIED");
                throw;
            }
        }
    }
}
