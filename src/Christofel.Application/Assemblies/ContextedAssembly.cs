using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

namespace Christofel.Application.Assemblies
{
    public class ContextedAssembly
    {
        private Assembly? _assembly;
        private AssemblyLoadContext? _context;
        
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
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public WeakReference Detach()
        {
            WeakReference weakReference = new WeakReference(Context);
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