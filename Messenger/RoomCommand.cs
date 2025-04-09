using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer
{
    public class RoomCommand : ICommand
    {
        public string Name => "/room";

        public string Description => throw new NotImplementedException();

        public async Task Execute(Client client, Server server, params string[] args)
        {
            try
            {
                var tag = server.CreateRoom();
                await client.SendMessageToClient($"CREATE ACCEPTED {tag}");
            }
            catch
            {
                await client.SendMessageToClient($"CREATE DENIED");
                throw;
            }
        }
    }
}