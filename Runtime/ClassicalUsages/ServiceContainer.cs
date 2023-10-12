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
        static Dictionary<Type, object> singletons = new();
        static Dictionary<(Type desiredType, Type declaringType), Info> lutInfos = new();
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
                    throw new ArgumentException($"type {type} doesn't has an implement");
                }
                if (subtypes.Length > 1)
                {
                    Debug.LogWarning($"type {type} exists more than one implements");
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
            // Debug.Log($"{unboundResultType} {resultTypeArguments.Length}");
            return unboundResultType.MakeGenericType(resultTypeArguments);
        }


        static Info GetInfoWithCache(Type desiredType, Type declaringType)
        {
            Info info;
            if (lutInfos.TryGetValue((desiredType, declaringType), out info)) return info; // first in order

            foreach (var interfType in declaringType.GetInterfaces())
            {
                if (lutInfos.TryGetValue((desiredType, interfType), out info)) return info;
            }
            for (var curType = declaringType.BaseType; curType != null; curType = curType.BaseType)
            {
                if (lutInfos.TryGetValue((desiredType, curType), out info)) return info;
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
            if (resultType.IsAbstract || resultType.IsInterface)
            {
                var resultType2 = FindTargetType(resultType);
                if (resultType == resultType2)
                {
                    throw new($"find {desiredType} returns a not implemented result");
                }
                info.resultType = resultType2;
            }

            return lutInfos[(desiredType, declaringType)] = info;
        }


        public static object Get(Type desiredType, Type declaringType = null)
        {
            var info = GetProperInfoAndCache(desiredType, declaringType);

            if (info.scopeMode == Single)
            {
                if (!singletons.TryGetValue(info.resultType, out var inst))
                {
                    singletons[info.resultType] = inst = Activator.CreateInstance(info.resultType);
                }
                return inst;
            }
            else
            {
                return Activator.CreateInstance(info.resultType);
            }
        }

        public static T Get<T>()
        {
            var inst = Get(typeof(T));
            Debug.Log($"inst {inst} to {typeof(T)}");
            return (T)Get(typeof(T));
        }

        public static void AddTransient<TContract, TResult>(Type declaringType = null)
        where TResult : TContract
        {
            var typePair = (typeof(TContract), declaringType);
            lutInfos[typePair] = new()
            {
                resultType = typeof(TResult),
                scopeMode = Transient,
            };
        }

        public static void AddSingle<TContract, TResult>(Type declaringType = null, bool noLazy = false)
        where TResult : TContract
        {
            var typePair = (typeof(TContract), declaringType);
            lutInfos[typePair] = new()
            {
                resultType = typeof(TResult),
                scopeMode = Single,
            };
            if (noLazy) Get<TContract>();
        }

        public static ServiceScopeMode GetScopeMode<TContract>(Type declaringType = null)
        {
            var info = GetInfoWithCache(typeof(TContract), declaringType);
            return info.scopeMode;
        }

    }

    struct Info
    {
        public Type resultType;
        public ServiceScopeMode scopeMode;
    }
}