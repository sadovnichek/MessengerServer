using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer
{
    public class QuitCommand : ICommand
    {
        public string Name => "/quit";

        public string Description => throw new NotImplementedException();

        public async Task Execute(Client client, Server server, params string[] args)
        {
            try
            {
                if (!server.TryGetRoom(client.CurrentRoomTag, out var clientRoom))
                    throw new InvalidOperationException();
                await clientRoom.RemoveClientFromRoom(client);
                client.SetStateOnQuit();
                await client.SendMessageToClient($"QIUT ACCEPTED");
            }
            catch
            {
                await client.SendMessageToClient($"QIUT DENIED");
                throw;
            }
        }
    }
}
