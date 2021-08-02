using System.Collections.Generic;
using Christofel.CommandsLib.Commands;

namespace Christofel.CommandsLib.Handlers
{
    public interface ICommandsGroupProvider
    {
        public IEnumerable<ICommandGroup> GetGroups();
    }
}