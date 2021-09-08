using System;

namespace Christofel.Application.Extensions
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Check whether the type given implements the given interface
        /// </summary>
        /// <param name="type"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool ImplementsInterface<T>(this Type type)
        {
            if (type.IsAbstract || !type.IsClass)
            {
                return false;
            }
            
            foreach (Type iface in type.GetInterfaces())
            {
                if (iface == typeof(T))
                {
                    return true;
                }
            }

            return false;
        }   
    }
}