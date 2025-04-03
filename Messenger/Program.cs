using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;

namespace MessengerServer
{
    public class Program
    {
        public static async Task Main()
        {
            var server = new Server();
            await server.Run();
        }
    }
}