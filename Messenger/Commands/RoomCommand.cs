using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer.Commands
{
    public class RoomCommand : ICommand
    {
        public string Name => "/room";

        public string Description => throw new NotImplementedException();

        public async Task Execute(Client client, Server server, params string[] args)
        {
            var tag = server.CreateRoom();
            await server.SendMessageToClient(client, $"CREATE ACCEPTED {tag}");
        }
    }
}