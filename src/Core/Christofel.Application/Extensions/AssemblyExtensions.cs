using System;
using System.Reflection;

namespace Christofel.Application.Extensions
{
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Obtain class implementing given interface
        /// </summary>
        /// <param name="assembly"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
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