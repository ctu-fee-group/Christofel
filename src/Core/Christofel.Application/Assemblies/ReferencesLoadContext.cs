using System;
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

        private readonly string[] _loadAlways = new[]
        {
            "Christofel.CommandsLib",
            "Remora.Commands",
            "Remora.Discord.Commands",
            "Christofel.BaseLib.Implementations",
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
            string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadAssemblyFromFileByStream(ctx, assemblyPath);
            }

            assemblyPath = Path.Combine(_pluginLoadDirectory, assemblyName.Name + ".dll");
            if (File.Exists(assemblyPath))
            {
                return LoadAssemblyFromFileByStream(ctx, assemblyPath);
            }

            return null;
        }

        private IntPtr LoadUnmanagedDllAssembly(Assembly? assembly, string unmanagedDllName)
        {
            string? libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }

        private Assembly LoadAssemblyFromFileByStream(AssemblyLoadContext ctx, string fileName)
        {
            var symbolsPath = Path.ChangeExtension(fileName, ".pdb");
            Stream? symbolsStream = null;

            if (File.Exists(symbolsPath))
            {
                symbolsStream = GetAssemblyMemoryStream(symbolsPath);
            }

            var assemblyStream = GetAssemblyMemoryStream(fileName);
            var assembly = ctx.LoadFromStream(assemblyStream, symbolsStream);
            
            assemblyStream.Dispose();
            symbolsStream?.Dispose();

            return assembly;
        }

        public MemoryStream GetAssemblyMemoryStream(string fileName)
        {
            var fileData = File.ReadAllBytes(fileName);
            return new MemoryStream(fileData);
        }
    }
}