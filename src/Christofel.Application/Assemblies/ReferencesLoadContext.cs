using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Christofel.Application.Assemblies
{
    /// <summary>
    /// AssemblyLoadContext with awareness of correct references.
    /// </summary>
    /// <remarks>
    /// Holds information about dlls that should be shared in the whole application.
    /// Tries to load references using AssemblyDependencyResolver,
    /// if that fails, tries to look for a dll in directory of the assembly
    /// </remarks>
    public class ReferencesLoadContext : AssemblyLoadContext
    {
        private readonly string _pluginLoadDirectory;
        private readonly AssemblyDependencyResolver _resolver;
        private readonly string[] _sharedAssemblies = new[]
        {
            // Global
            "System.Runtime",
            // Base lib
            "Christofel.BaseLib",
            // Discord
            "Discord.Net",
            "Discord.Net.Webhook",
            "Discord.Net.Core", 
            "Discord.Net.Rest", 
            "Discord.Net.WebSocket",
            "Discord.Net.Labs",
            "Discord.Net.Labs.Webhook",
            "Discord.Net.Labs.Core", 
            "Discord.Net.Labs.Rest", 
            "Discord.Net.Labs.WebSocket",
            // MS
            // Configuration
            "Microsoft.Extensions.Configuration",
            "Microsoft.Extensions.Configuration.Abstractions",
            "Microsoft.Extensions.Primitives",
            // Logging
            "Microsoft.Extensions.Logging",
            // DI
            "Microsoft.Extensions.DependencyInjection",
            "Microsoft.Extensions.DependencyInjection.Abstractions",
            // EF Core
            "Microsoft.EntityFrameworkCore",
            "Microsoft.EntityFrameworkCore.Relational",
            "Microsoft.EntityFrameworkCore.Abstractions",
        };

        public ReferencesLoadContext(string pluginPath)
            : base(null, true)
        {
            _pluginLoadDirectory = Path.GetDirectoryName(pluginPath) ?? "";
            _resolver = new AssemblyDependencyResolver(_pluginLoadDirectory);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            if (_sharedAssemblies.Contains(assemblyName.Name))
            {
                return null;
            }
            
            string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            assemblyPath = Path.Combine(_pluginLoadDirectory, assemblyName.Name + ".dll");
            if (File.Exists(assemblyPath))
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string? libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }
    }
}