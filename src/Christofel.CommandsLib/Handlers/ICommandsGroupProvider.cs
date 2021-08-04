using System.Collections.Generic;
using Christofel.CommandsLib.Commands;

namespace Christofel.CommandsLib.Handlers
{
    /// <summary>
    /// Provider ICommandGroup for ICommandRegistrator
    /// </summary>
    public interface ICommandsGroupProvider
    {
        public IEnumerable<ICommandGroup> GetGroups();
    }
}