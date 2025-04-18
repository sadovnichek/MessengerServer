using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer
{
    public enum State
    {
        Connected = 0,
        Joined = 1
    }

    public class Client
    {
        public Guid Guid { get; }

        public string Username { get; set; }

        public EndPoint RemoteEndPoint => client.Client.RemoteEndPoint;

        public StreamWriter Writer { get; }

        public StreamReader Reader { get; }

        public bool Connected => client.Connected;

        public string CurrentRoomTag 
        { 
            get
            {
                if (State != State.Joined)
                    throw new InvalidCastException("User is not joined to any room");
                return currentRoomTag;
            }
        }

        public State State { get; private set; }

        private TcpClient client;

        private string currentRoomTag;

        public Client(TcpClient client)
        {
            Guid = Guid.NewGuid();
            this.client = client;
            var stream = client.GetStream();
            Reader = new StreamReader(stream);
            Writer = new StreamWriter(stream);
            Username = Guid.ToString().Substring(0, 7);
            currentRoomTag = string.Empty;
        }

        public void SetStateOnJoin(string roomTag)
        {
            State = State.Joined;
            currentRoomTag = roomTag;
        }

        public void SetStateOnQuit()
        {
            State = State.Connected;
            currentRoomTag = string.Empty;
        }

        public void Disconnect()
        {
            client.Close();
        }
    }
}
