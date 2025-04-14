using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessengerServer.Commands;

namespace MessengerServer
{
    public static class CommandHelper
    {
        private static Type commandInterfaceType;

        static CommandHelper()
        {
            commandInterfaceType = typeof(ICommand);
        }

        public static IEnumerable<ICommand> GetCommands()
        {
            return AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => commandInterfaceType.IsAssignableFrom(type) && type.IsClass)
                .Select(CreateObject<ICommand>);
        }

        private static T CreateObject<T>(Type type)
        {
            return (T)type.GetConstructor([]).Invoke([]);
        }
    }
}
