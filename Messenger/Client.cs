using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer
{
    public class Client
    {
        public Guid Guid { get; }

        public string Username { get; set; }

        public EndPoint RemoteEndPoint => client.Client.RemoteEndPoint;

        public StreamWriter Writer { get; }

        public StreamReader Reader { get; }

        public bool Connected => client.Connected;

        public string CurrentRoomTag { get; set; }

        private TcpClient client;

        public Client(TcpClient client)
        {
            Guid = Guid.NewGuid();
            this.client = client;
            var stream = client.GetStream();
            Reader = new StreamReader(stream);
            Writer = new StreamWriter(stream);
            Username = Guid.ToString().Substring(0, 6);
        }

        public void Disconnect()
        {
            client.Close();
        }

        public async Task SendMessageToClient(string message)
        {
            await Writer.WriteLineAsync(message);
            await Writer.FlushAsync();
        }
    }
}
