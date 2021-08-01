using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Christofel.CommandsLib.Reflection
{
    // From https://github.com/tdupont750/tact.net/blob/master/framework/src/Tact/Reflection/EfficientInvoker.cs
    public sealed class EfficientInvoker
    {
        private static readonly ConcurrentDictionary<ConstructorInfo, Func<object[], object>> ConstructorToWrapperMap
            = new ConcurrentDictionary<ConstructorInfo, Func<object[], object>>();

        private static readonly ConcurrentDictionary<Type, EfficientInvoker> TypeToWrapperMap
            = new ConcurrentDictionary<Type, EfficientInvoker>();

        private static readonly ConcurrentDictionary<MethodKey, EfficientInvoker> MethodToWrapperMap
            = new ConcurrentDictionary<MethodKey, EfficientInvoker>(MethodKeyComparer.Instance);

        private readonly Func<object?, object?[]?, object?> _func;

        private EfficientInvoker(Func<object?, object?[]?, object?> func)
        {
            _func = func;
        }

        public Delegate AsDelegate()
        {
            return _func;
        }
        
        public static Func<object[], object> ForConstructor(ConstructorInfo constructor)
        {
            if (constructor == null)
                throw new ArgumentNullException(nameof(constructor));

            return ConstructorToWrapperMap.GetOrAdd(constructor, t =>
            {
                CreateParamsExpressions(constructor, out ParameterExpression argsExp, out Expression[] paramsExps);

                var newExp = Expression.New(constructor, paramsExps);
                var resultExp = Expression.Convert(newExp, typeof(object));
                var lambdaExp = Expression.Lambda(resultExp, argsExp);
                var lambda = lambdaExp.Compile();
                return (Func<object[], object>)lambda;
            });
        }

        public static EfficientInvoker ForDelegate(Delegate del)
        {
            if (del == null)
                throw new ArgumentNullException(nameof(del));

            var type = del.GetType();
            return TypeToWrapperMap.GetOrAdd(type, t =>
            {
                var method = del.GetMethodInfo();
                var wrapper = CreateMethodWrapper(t, method, true);
                return new EfficientInvoker(wrapper);
            });
        }

        public static EfficientInvoker ForMethod(Type type, string methodName)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (methodName == null)
                throw new ArgumentNullException(nameof(methodName));

            var key = new MethodKey(type, methodName);
            return MethodToWrapperMap.GetOrAdd(key, k =>
            {
                var method = k.Type.GetTypeInfo().GetMethod(k.Name);
                if (method == null)
                {
                    throw new InvalidOperationException();
                }
                
                var wrapper = CreateMethodWrapper(k.Type, method, false);
                return new EfficientInvoker(wrapper);
            });
        }

        public static EfficientInvoker ForProperty(Type type, string propertyName)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            var key = new MethodKey(type, propertyName);
            return MethodToWrapperMap.GetOrAdd(key, k =>
            {
                var wrapper = CreatePropertyWrapper(type, propertyName);
                return new EfficientInvoker(wrapper);
            });
        }

        public object? Invoke(object? target, params object?[]? args)
        {
            return _func(target, args);
        }

        public async Task<object> InvokeAsync(object? target, params object?[]? args)
        {
            var result = _func(target, args);
            var task = result as Task;
            if (task is null)
                return Task.CompletedTask;

            if (!task.IsCompleted)
                await task.ConfigureAwait(false);

            return task;
        }

        private static Func<object?, object?[]?, object?> CreateMethodWrapper(Type type, MethodInfo method, bool isDelegate)
        {
            CreateParamsExpressions(method, out ParameterExpression argsExp, out Expression[] paramsExps);

            var targetExp = Expression.Parameter(typeof(object), "target");
            var castTargetExp = Expression.Convert(targetExp, type);
            var invokeExp = isDelegate
                ? (Expression)Expression.Invoke(castTargetExp, paramsExps)
                : Expression.Call(castTargetExp, method, paramsExps);

            LambdaExpression lambdaExp;
            
            if (method.ReturnType != typeof(void))
            {
                var resultExp = Expression.Convert(invokeExp, typeof(object));
                lambdaExp = Expression.Lambda(resultExp, targetExp, argsExp);
            }
            else
            {
                var constExp = Expression.Constant(null, typeof(object));
                var blockExp = Expression.Block(invokeExp, constExp);
                lambdaExp = Expression.Lambda(blockExp, targetExp, argsExp);
            }

            var lambda = lambdaExp.Compile();
            return (Func<object?, object?[]?, object?>)lambda;
        }

        private static void CreateParamsExpressions(MethodBase method, out ParameterExpression argsExp, out Expression[] paramsExps)
        {
            var parameters = method.GetParameters().Select(x => x.ParameterType).ToList();

            argsExp = Expression.Parameter(typeof(object[]), "args");
            paramsExps = new Expression[parameters.Count];

            for (var i = 0; i < parameters.Count; i++)
            {
                var constExp = Expression.Constant(i, typeof(int));
                var argExp = Expression.ArrayIndex(argsExp, constExp);
                paramsExps[i] = Expression.Convert(argExp, parameters[i]);
            }
        }
        
        private static Func<object?, object?[]?, object?> CreatePropertyWrapper(Type type, string propertyName)
        {
            var property = type.GetRuntimeProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException();
            }
            
            var targetExp = Expression.Parameter(typeof(object), "target");
            var argsExp = Expression.Parameter(typeof(object[]), "args");
            var castArgExp = Expression.Convert(targetExp, type);
            var propExp = Expression.Property(castArgExp, property);
            var castPropExp = Expression.Convert(propExp, typeof(object));
            var lambdaExp = Expression.Lambda(castPropExp, targetExp, argsExp);
            var lambda = lambdaExp.Compile();
            return (Func<object?, object?[]?, object?>) lambda;
        }

        private class MethodKeyComparer : IEqualityComparer<MethodKey>
        {
            public static readonly MethodKeyComparer Instance = new MethodKeyComparer();

            public bool Equals(MethodKey x, MethodKey y)
            {
                return x.Type == y.Type &&
                       StringComparer.Ordinal.Equals(x.Name, y.Name);
            }

            public int GetHashCode(MethodKey key)
            {
                var typeCode = key.Type.GetHashCode();
                var methodCode = key.Name.GetHashCode();
                return CombineHashCodes(typeCode, methodCode);
            }

            // From System.Web.Util.HashCodeCombiner
            private static int CombineHashCodes(int h1, int h2)
            {
                return ((h1 << 5) + h1) ^ h2;
            }
        }

        private struct MethodKey
        {
            public MethodKey(Type type, string name)
            {
                Type = type;
                Name = name;
            }

            public readonly Type Type;
            public readonly string Name;
        }
    }
}