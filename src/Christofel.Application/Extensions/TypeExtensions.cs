using System;

namespace Christofel.Application.Extensions
{
    public static class TypeExtensions
    {
        public static bool ImplementsInterface<T>(this Type type)
        {
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