using System;
using System.Collections;
using System.Collections.Generic;
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
            "System.ComponentModel",
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
            "Microsoft.Extensions.Logging.Abstractions",
            // DI
            "Microsoft.Extensions.DependencyInjection",
            "Microsoft.Extensions.DependencyInjection.Abstractions",
            // EF Core
            "Microsoft.EntityFrameworkCore",
            "Microsoft.EntityFrameworkCore.Relational",
            "Microsoft.EntityFrameworkCore.Abstractions",
        };

        private readonly string[] _loadAlways = new[]
        {
            "Christofel.CommandsLib",
            "Christofel.BaseLib.Implementations"
        };

        public ReferencesLoadContext(string pluginPath)
            : base(null, true)
        {
            _pluginLoadDirectory = Path.GetDirectoryName(pluginPath) ?? "";
            _resolver = new AssemblyDependencyResolver(_pluginLoadDirectory);

            Resolving += LoadAssembly;
            ResolvingUnmanagedDll += LoadUnmanagedDllAssembly;
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            if (_loadAlways.Contains(assemblyName.Name))
            {
                return LoadAssembly(this, assemblyName) ?? base.Load(assemblyName);
            }

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            IntPtr pointer = LoadUnmanagedDllAssembly(null, unmanagedDllName);
            return pointer == IntPtr.Zero ? base.LoadUnmanagedDll(unmanagedDllName) : pointer;
        }

        private Assembly? LoadAssembly(AssemblyLoadContext ctx, AssemblyName assemblyName)
        {
            if (_sharedAssemblies.Contains(assemblyName.Name))
            {
                return null;
            }
            
            string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return ctx.LoadFromAssemblyPath(assemblyPath);
            }

            assemblyPath = Path.Combine(_pluginLoadDirectory, assemblyName.Name + ".dll");
            if (File.Exists(assemblyPath))
            {
                return ctx.LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }

        private IntPtr LoadUnmanagedDllAssembly(Assembly? assembly, string unmanagedDllName)
        {
            File.AppendAllText("/tmp/resolve_unmanaged.txt", "\n" + unmanagedDllName);

            string? libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }
    }
}