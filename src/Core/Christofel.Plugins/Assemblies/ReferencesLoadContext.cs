//
//   ReferencesLoadContext.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace Christofel.Plugins.Assemblies
{
    /// <summary>
    /// AssemblyLoadContext with awareness of correct references.
    /// </summary>
    /// <remarks>
    /// Tries to load references using AssemblyDependencyResolver,
    /// if that fails, tries to look for a dll in directory of the assembly.
    /// </remarks>
    public class ReferencesLoadContext : AssemblyLoadContext
    {
        private readonly string _pluginLoadDirectory;
        private readonly AssemblyDependencyResolver _resolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferencesLoadContext"/> class.
        /// </summary>
        /// <param name="pluginPath">Path to the assembly representing the plugin.</param>
        public ReferencesLoadContext(string pluginPath)
            : base(null, true)
        {
            _pluginLoadDirectory = Path.GetDirectoryName(pluginPath) ?? string.Empty;
            _resolver = new AssemblyDependencyResolver(_pluginLoadDirectory);

            Resolving += LoadAssembly;
            ResolvingUnmanagedDll += LoadUnmanagedDllAssembly;
        }

        /// <inheritdoc />
        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var pointer = LoadUnmanagedDllAssembly(null, unmanagedDllName);
            return pointer == IntPtr.Zero
                ? base.LoadUnmanagedDll(unmanagedDllName)
                : pointer;
        }

        /// <summary>
        /// Attempts to load assembly using <see cref="_resolver"/> and by finding the dll inside of the plugin directory.
        /// </summary>
        /// <param name="ctx">The context that the assembly should be loaded to.</param>
        /// <param name="assemblyName">Name of the assembly to load.</param>
        /// <returns>The loaded assembly, null if it was not found.</returns>
        private Assembly? LoadAssembly(AssemblyLoadContext ctx, AssemblyName assemblyName)
        {
            var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
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

        /// <summary>
        /// Attempts to load unmanaged assembly using <see cref="_resolver"/>.
        /// </summary>
        /// <param name="assembly">The assembly to load into.</param>
        /// <param name="unmanagedDllName">Name of the unmanaged library.</param>
        /// <returns>Pointer to the library, <see cref="IntPtr.Zero"/> if not found.</returns>
        private IntPtr LoadUnmanagedDllAssembly(Assembly? assembly, string unmanagedDllName)
        {
            var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Loads assembly file along with its symbols (if they exist) into memory and loads it into the specified context.
        /// </summary>
        /// <remarks>
        /// By using this method, both dll and pdb files can be replaced safely.
        /// </remarks>
        /// <param name="ctx">The context to load the assembly into.</param>
        /// <param name="fileName">Path to the assembly.</param>
        /// <returns>Loaded assembly.</returns>
        public Assembly LoadAssemblyFromFileByStream(AssemblyLoadContext ctx, string fileName)
        {
            var symbolsPath = Path.ChangeExtension(fileName, ".pdb");
            Stream? symbolsStream = null;

            if (File.Exists(symbolsPath))
            {
                symbolsStream = File.OpenRead(symbolsPath);
            }

            var assemblyStream = File.OpenRead(fileName);
            var assembly = ctx.LoadFromStream(assemblyStream, symbolsStream);

            assemblyStream.Dispose();
            symbolsStream?.Dispose();

            return assembly;
        }
    }
}