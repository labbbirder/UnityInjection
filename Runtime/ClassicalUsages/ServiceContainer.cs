using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace com.bbbirder.injection
{
    using static ServiceScopeMode;
    public enum ServiceScopeMode
    {
        Single,
        Transient,
    }


    // public interface IServiceContainer
    // {
    //     public void AddTransient<TContract, TResult>();
    //     public void AddSingle<TContract, TResult>();
    //     public ServiceScopeMode GetScopeMode<TContract>();
    //     public T Get<T>();
    // }


    public static class ServiceContainer
    //: IServiceContainer
    {
        static ServiceScopeMode DefaultScopeMode => Single;
        internal static Dictionary<Type, object> singletons = new();
        internal static Dictionary<(Type desiredType, Type declaringType), Info> lutInfos = new();
        public static void ClearInstances()
        {
            singletons.Clear();
        }
        static Type FindImplementSubclass(Type type)
        {
            if (type.IsAbstract || type.IsInterface)
            {
                var subtypes = Retriever.GetAllSubtypes(type)
                    .Append(type)
                    .Where(t => !t.IsInterface)
                    .Where(t => !t.IsAbstract)
                    .ToArray()
                    ;
                if (subtypes.Length == 0)
                {
                    // throw new ArgumentException($"type {type} doesn't has an implement");
                    return null;
                }
                if (subtypes.Length > 1)
                {
                    var orderedTypeGroups = subtypes
                        .GroupBy(tp => tp.GetCustomAttribute<OrderDIAttribute>()?.order ?? 0)
                        .OrderBy(g => g.Key);
                    foreach (var g in orderedTypeGroups)
                    {
                        if (g.Count() > 1)
                        {
                            Debug.LogWarning($"type {type} exists more than one implements, sharing a priority order {g.Key}: {string.Join(",", g.AsEnumerable().Select(t => t.FullName))}");
                        }
                        return g.First();
                    }
                }
                return subtypes[0];
            }
            return type;
        }
        static Type FindTargetType(Type type)
        {
            if (!type.IsGenericType) return FindImplementSubclass(type);

            if (type.IsGenericTypeDefinition)
            {
                throw new ArgumentException($"a unbound generic type {type}");
            }
            var unboundType = type.GetGenericTypeDefinition();
            var unboundTypeArguments = unboundType.GetGenericArguments();

            var resultTypeArguments = new Type[type.GenericTypeArguments.Length];

            for (var i = 0; i < type.GenericTypeArguments.Length; i++)
            {
                var typeArg = type.GenericTypeArguments[i];
                var unboudnTypeArg = unboundTypeArguments[i];
                var notImpl = typeArg.IsAbstract || typeArg.IsInterface;
                // Debug.Log($"{typeArg} {unboudnTypeArg} {notImpl} {unboudnTypeArg.GenericParameterAttributes} {unboudnTypeArg.GenericParameterAttributes | GenericParameterAttributes.Covariant}");
                if (notImpl && (unboudnTypeArg.GenericParameterAttributes & GenericParameterAttributes.Covariant) == 0)
                {
                    throw new($"type arg {unboudnTypeArg} must has a 'out' modifier in {unboundType}");
                }
                resultTypeArguments[i] = FindTargetType(typeArg);
            }
            var unboundResultType = FindImplementSubclass(unboundType);
            if (unboundResultType is null) return unboundResultType;
            // Debug.Log($"{unboundResultType} {resultTypeArguments.Length}");
            return unboundResultType.MakeGenericType(resultTypeArguments);
        }


        static Info GetInfoWithCache(Type desiredType, Type declaringType)
        {
            Info info;
            if (lutInfos.TryGetValue((desiredType, declaringType), out info)) return info; // first in order

            if (declaringType != null)
            {
                foreach (var interfType in declaringType.GetInterfaces())
                {
                    if (lutInfos.TryGetValue((desiredType, interfType), out info)) return info;
                }
                for (var curType = declaringType.BaseType; curType != null; curType = curType.BaseType)
                {
                    if (lutInfos.TryGetValue((desiredType, curType), out info)) return info;
                }
            }

            if (lutInfos.TryGetValue((desiredType, null), out info)) return info; // default cache

            info.resultType = FindTargetType(desiredType);
            info.scopeMode = Single; // default scope mode
            lutInfos[(desiredType, null)] = info;

            return info;
        }


        static Info GetProperInfoAndCache(Type desiredType, Type declaringType)
        {
            var info = GetInfoWithCache(desiredType, declaringType);

            var resultType = info.resultType;
            if (resultType is null)
            {
                // dont cache on missing implementation
                return info;
            }

            if (resultType.IsAbstract || resultType.IsInterface)
            {
                var info2 = GetProperInfoAndCache(resultType, null);
                if (resultType == info2.resultType || info2.resultType.IsAssignableFrom(resultType))
                {
                    throw new($"find {desiredType} returns a not implemented result");
                }
                info.resultType = info2.resultType;
            }

            return lutInfos[(desiredType, declaringType)] = info;
        }

        static object CreateInstance(Info info)
        {
            if (info.creator != null) return info.creator();
            if (info.resultType == null) return null;
            return Activator.CreateInstance(info.resultType, info.constructorArguments);
        }

        public static object Get(Type desiredType, Type declaringType = null, bool throwOnNoImplementations = true)
        {
            var info = GetProperInfoAndCache(desiredType, declaringType);

            if (info.resultType is null)
            {
                if(throwOnNoImplementations) 
                    throw new ArgumentException($"type {desiredType} doesn't has an implement");
                return null;
            }

            if (info.scopeMode == Single)
            {
                if (!singletons.TryGetValue(info.resultType, out var inst))
                {
                    singletons[info.resultType] = inst = CreateInstance(info);
                }
                return inst;
            }
            else
            {
                return CreateInstance(info);
            }
        }

        public static T Get<T>(Type declaringType = null)
        {
            var inst = Get(typeof(T), declaringType);
            return (T)inst;
        }

        public static ServiceScopeMode GetScopeMode<TContract>(Type declaringType = null)
        {
            var info = GetInfoWithCache(typeof(TContract), declaringType);
            return info.scopeMode;
        }

        /// <summary>
        /// for members declared in type:<typeparamref name="T"/> ...
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static BindDeclaringContext In<T>()
        {
            return In(typeof(T));
        }

        /// <summary>
        /// for members declared in <paramref name="targetType"/> ...
        /// </summary>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static BindDeclaringContext In(Type targetType)
        {
            return new BindDeclaringContext(targetType);
        }

        /// <summary>
        /// for any members with type:<typeparamref name="T"/> ...
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static BindSourceContext<T> Bind<T>()
        {
            return new BindDeclaringContext(null).Bind<T>(); // any declaring type
        }

    }

    public struct BindDeclaringContext
    {
        Type declaringType;
        internal BindDeclaringContext(Type declaringType)
        {
            this.declaringType = declaringType;
        }
        /// <summary>
        /// for members with type <typeparamref name="TSource"/>...
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public BindSourceContext<TSource> Bind<TSource>()
        {
            return new BindSourceContext<TSource>(declaringType);
        }
    }

    public struct BindSourceContext<TSource>
    {
        Type declaringType;
        Type desiredType;
        bool noLazy;
        ServiceScopeMode scopeMode;
        internal BindSourceContext(Type declaringType)
        {
            this.declaringType = declaringType;
            this.desiredType = typeof(TSource);
            this.scopeMode = ServiceScopeMode.Single;
            this.noLazy = false;
        }
        /// <summary>
        /// returns a new instance when get
        /// </summary>
        /// <returns></returns>
        public BindSourceContext<TSource> AsTransient()
        {
            this.scopeMode = ServiceScopeMode.Transient;
            return this;
        }
        /// <summary>
        /// return the singleton when get
        /// </summary>
        /// <param name="noLazy">instantiate immediately</param>
        /// <returns></returns>
        public BindSourceContext<TSource> AsSingle(bool noLazy = false)
        {
            this.scopeMode = ServiceScopeMode.Single;
            this.noLazy = noLazy;
            return this;
        }
        /// <summary>
        /// bind members with type <typeparamref name="TSource"/> to type <typeparamref name="TDest"/>
        /// </summary>
        /// <param name="arguments"></param>
        /// <typeparam name="TDest"></typeparam>
        public void To<TDest>(params object[] arguments) where TDest : TSource
        {
            var typePair = (desiredType, declaringType);
            ServiceContainer.lutInfos[typePair] = new()
            {
                resultType = typeof(TDest),
                scopeMode = scopeMode,
                constructorArguments = arguments,
                creator = null,
            };
            if (noLazy && scopeMode == ServiceScopeMode.Single)
            {
                ServiceContainer.Get(desiredType, declaringType);
            }
        }

        public void To<TDest>(Func<TDest> creator) where TDest : TSource
        {
            var typePair = (desiredType, declaringType);
            Debug.Log($"to {scopeMode} {typeof(TDest)}");
            ServiceContainer.lutInfos[typePair] = new()
            {
                resultType = typeof(TDest),
                scopeMode = scopeMode,
                // constructorArguments = arguments,
                creator = () => creator()
            };
            if (noLazy && scopeMode == ServiceScopeMode.Single)
            {
                ServiceContainer.Get(desiredType, declaringType);
            }
        }
    }

    struct Info
    {
        public Type resultType;
        public ServiceScopeMode scopeMode;
        public object[] constructorArguments;
        public Func<object> creator;
    }
}