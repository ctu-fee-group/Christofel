using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Christofel.CommandsLib.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Christofel.CommandsLib.Handlers
{
    /// <summary>
    /// Service for providing ICommandGroup.
    /// </summary>
    /// <remarks>
    /// Command groups should be registered using RegisterGroup.
    /// For DI, RegisterGroupType can be used. With DI Options should be used.
    /// CommandGroupsService should be treated as IOptions and it should be configured using
    /// <c>IServiceCollection.Configure<CommandsGroupsService>((groupsService) => groupsService.RegisterGroupType(...))</c>
    /// </remarks>
    public class CommandGroupsService
    {
        private readonly List<ICommandGroup> _groups;
        private readonly List<Type> _groupTypes;
        private bool _initialized;
        
        public CommandGroupsService()
        {
            _groups = new List<ICommandGroup>();
            _groupTypes = new List<Type>();
        }

        public void RegisterGroupType(Type commandGroupType)
        {
            if (!commandGroupType.IsAssignableTo(typeof(ICommandGroup)))
            {
                throw new ArgumentException($@"Type {commandGroupType.Name} cannot be added as it doesn't inherit from ICommandGroup");
            }
            
            _groupTypes.Add(commandGroupType);
        }

        public void RegisterGroup(ICommandGroup group)
        {
            _groups.Add(group);
        }

        public IEnumerable<ICommandGroup> GetGroups(IServiceProvider? provider)
        {
            if (!_initialized && _groupTypes.Count > 0)
            {
                if (provider is null)
                {
                    throw new InvalidOperationException("Register by type can only be done if service provider is used");
                }

                _groups.AddRange(_groupTypes.Select(x => (ICommandGroup)provider.GetRequiredService(x)));
                
                _initialized = true;
            }

            return _groups.AsReadOnly();
        }

    }
}