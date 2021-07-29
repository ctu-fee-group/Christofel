using System;
using System.Reflection;

namespace Christofel.Application.Extensions
{
    public static class AssemblyExtensions
    {
        public static Type GetTypeImplementing<T>(this Assembly assembly)
        {
            foreach (Type type in assembly.ExportedTypes)
            {
                if (type.ImplementsInterface<T>())
                {
                    return type;
                }
            }

            throw new InvalidOperationException("Could not find type implementing specified type in given assembly");
        }
    }
}