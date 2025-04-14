using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer.Commands
{
    public interface ICommand
    {
        string Name { get; }

        string Description { get; }

        Task Execute(Client client, Server server, params string[] args);
    }
}
