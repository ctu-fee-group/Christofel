//
//   ContextedAssembly.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace Christofel.Plugins.Assemblies
{
    /// <summary>
    /// Class for holding loaded assembly inside of <see cref="AssemblyLoadContext"/>.
    /// </summary>
    public class ContextedAssembly
    {
        private readonly object _detachLock = new object();
        private Assembly? _assembly;
        private AssemblyLoadContext? _context;
        private WeakReference? _weakReference;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextedAssembly"/> class.
        /// </summary>
        /// <param name="context">The context assembly is loaded in.</param>
        /// <param name="assembly">The assembly that was loaded into ALC.</param>
        public ContextedAssembly(AssemblyLoadContext context, Assembly assembly)
        {
            _assembly = assembly;
            _context = context;
        }

        /// <summary>
        /// Gets <see cref="AssemblyLoadContext"/> the assemby is loaded in.
        /// </summary>
        /// <exception cref="InvalidOperationException">Will be thrown if the assembly was already unloaded using <see cref="Detach"/>.</exception>
        public AssemblyLoadContext Context
        {
            get
            {
                if (_context == null)
                {
                    throw new InvalidOperationException("Assembly is detached");
                }

                return _context;
            }
        }

        /// <summary>
        /// Gets the wrapped assembly that was requested to be loaded.
        /// </summary>
        /// <exception cref="InvalidOperationException">Will be thrown if the assembly was already unloaded using <see cref="Detach"/>.</exception>
        public Assembly Assembly
        {
            get
            {
                if (_assembly == null)
                {
                    throw new InvalidOperationException("Assembly is detached");
                }

                return _assembly;
            }
        }

        /// <summary>
        /// Unloads AssemblyLoadContext and throws away it's reference.
        /// </summary>
        /// <returns>Weak reference to AssemblyLoadContext so it can be checked whether the AssemblyLoadContext was destroyed.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public WeakReference Detach()
        {
            lock (_detachLock)
            {
                if (_context == null)
                {
                    if (_weakReference == null)
                    {
                        throw new InvalidOperationException("Weak reference was not set even though it should be");
                    }

                    return _weakReference;
                }

                WeakReference weakReference = _weakReference = new WeakReference(Context);
                if (Context.IsCollectible)
                {
                    Context.Unload();
                }

                _assembly = null;
                _context = null;

                return weakReference;
            }
        }
    }
}