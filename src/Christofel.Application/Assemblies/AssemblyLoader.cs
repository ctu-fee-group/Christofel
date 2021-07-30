using System.Reflection;
using System.Runtime.CompilerServices;

namespace Christofel.Application.Assemblies
{
    public class AssemblyLoader
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ContextedAssembly Load(string path)
        {
            ReferencesLoadContext context = new ReferencesLoadContext(path);
            Assembly assembly = context.LoadFromAssemblyPath(path);

            return new ContextedAssembly(context, assembly);
        }
    }
}