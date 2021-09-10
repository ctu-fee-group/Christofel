//
//   AssemblyLoader.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace Christofel.Plugins.Assemblies
{
    /// <summary>
    /// Helper class for loading assemblies using <see cref="AssemblyLoadContext"/>.
    /// </summary>
    public class AssemblyLoader
    {
        /// <summary>
        /// Load assembly in custom AssemblyLoadContext
        /// along with correct references handling.
        /// </summary>
        /// <param name="path">Path to the assembly. Should be dll.</param>
        /// <returns>Assembly along with the context it is in.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ContextedAssembly Load(string path)
        {
            ReferencesLoadContext context = new ReferencesLoadContext(path);
            Assembly assembly = context.LoadAssemblyFromFileByStream(context, path);

            return new ContextedAssembly(context, assembly);
        }
    }
}