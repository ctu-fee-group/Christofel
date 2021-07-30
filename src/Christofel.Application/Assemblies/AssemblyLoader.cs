using System.Reflection;

namespace Christofel.Application.Assemblies
{
    public class AssemblyLoader
    {
        public static ContextedAssembly Load(string path)
        {
            ReferencesLoadContext context = new ReferencesLoadContext(path);
            Assembly assembly = context.LoadFromAssemblyPath(path);

            return new ContextedAssembly(context, assembly);
        }
    }
}