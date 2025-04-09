using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer
{
    public class JoinCommand : ICommand
    {
        public string Name => "/join";

        public string Description => throw new NotImplementedException();

        public async Task Execute(Client client, Server server, params string[] args)
        {
            if(!Validate(client.State, args))
                throw new InvalidOperationException();
            try
            {
                var roomTag = args[0];
                if (server.TryGetRoom(roomTag, out var room))
                {
                    await room.Join(client);
                    client.SetStateOnJoin(roomTag);
                    await client.SendMessageToClient($"JOIN ACCEPTED {room.Tag}");
                }
                else
                {
                    await client.SendMessageToClient($"JOIN DENIED. ROOM {roomTag} DOES NOT EXIST");
                }
            }
            catch
            {
                await client.SendMessageToClient($"JOIN DENIED");
                throw;
            }
        }

        private bool Validate(State clientState, params string[] args)
        {
            return args.Length == 1 && clientState != State.Joined;
        }
    }
}
