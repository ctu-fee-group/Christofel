using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace Christofel.Application.Assemblies
{
    /// <summary>
    /// Class for holding loaded assembly in AssemblyLoadContext
    /// </summary>
    public class ContextedAssembly
    {
        private Assembly? _assembly;
        private AssemblyLoadContext? _context;
        private readonly object _detachLock = new object();
        private WeakReference? _weakReference;
        
        public ContextedAssembly(AssemblyLoadContext context, Assembly assembly)
        {
            _assembly = assembly;
            _context = context;
        }

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
        /// Unloads AssemblyLoadContext and throws away it's reference
        /// </summary>
        /// <returns>Weak reference to AssemblyLoadContext so it can be checked whether the AssemblyLoadContext was destroyed</returns>
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