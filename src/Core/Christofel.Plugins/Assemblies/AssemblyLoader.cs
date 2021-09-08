using System.Reflection;
using System.Runtime.CompilerServices;

namespace Christofel.Plugins.Assemblies
{
    public class AssemblyLoader
    {
        /// <summary>
        /// Load assembly in custom AssemblyLoadContext
        /// along with correct references handling
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ContextedAssembly Load(string path)
        {
            ReferencesLoadContext context = new ReferencesLoadContext(path);
            Assembly assembly = context.LoadFromStream(context.GetAssemblyMemoryStream(path));

            return new ContextedAssembly(context, assembly);
        }
    }
}