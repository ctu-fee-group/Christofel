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
    public class DICommandGroupsProvider : ICommandsGroupProvider
    {
        private readonly List<Type> _groupTypes;
        
        public DICommandGroupsProvider()
        {
            _groupTypes = new List<Type>();
        }
        
        public IServiceProvider? Provider { get; set; }

        public void RegisterGroupType(Type commandGroupType)
        {
            if (!commandGroupType.IsAssignableTo(typeof(ICommandGroup)))
            {
                throw new ArgumentException($@"Type {commandGroupType.Name} cannot be added as it doesn't inherit from ICommandGroup");
            }
            
            _groupTypes.Add(commandGroupType);
        }

        public IEnumerable<ICommandGroup> GetGroups()
        {
            if (Provider is null)
            {
                throw new InvalidOperationException("Register by type can only be done if service provider is used");
            }

            return _groupTypes.Select(x => (ICommandGroup) Provider.GetRequiredService(x));
        }

    }
}