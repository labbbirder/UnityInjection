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


    public static class ServiceContainer //: IServiceContainer
    {
        static ServiceScopeMode DefaultScopeMode => Single;
        static Dictionary<Type, object> singletons = new();
        static Dictionary<Type, Info> lutInfos = new();
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
        static Type FindType(Type type)
        {
            if (!lutInfos.TryGetValue(type, out var info))
            {
                Type resultType = default;

                if (type.IsGenericType)
                {
                    if (type.IsGenericTypeDefinition)
                    {
                        throw new ArgumentException($"a unbound generic type {type}");
                    }
                    else
                    {
                        var unboundType = type.GetGenericTypeDefinition();
                        var unboundTypeArguments = unboundType.GetGenericArguments();

                        var resultTypeArguments = new Type[type.GenericTypeArguments.Length];
                        type.GenericTypeArguments.Select(FindType).ToArray();
                        for (var i = 0; i < type.GenericTypeArguments.Length; i++)
                        {
                            var typeArg = type.GenericTypeArguments[i];
                            var unboudnTypeArg = unboundTypeArguments[i];
                            var notImpl = typeArg.IsAbstract || typeArg.IsInterface;
                            Debug.Log($"{typeArg} {unboudnTypeArg} {notImpl} {unboudnTypeArg.GenericParameterAttributes} {(unboudnTypeArg.GenericParameterAttributes | GenericParameterAttributes.Covariant)}");
                            if (notImpl && (unboudnTypeArg.GenericParameterAttributes & GenericParameterAttributes.Covariant) == 0)
                            {
                                throw new($"type arg {unboudnTypeArg} must has a 'out' modifier in {unboundType}");
                            }
                            resultTypeArguments[i] = FindType(typeArg);
                        }
                        var unboundResultType = FindImplementSubclass(unboundType);
                        Debug.Log($"{unboundResultType} {resultTypeArguments.Length}");
                        resultType = unboundResultType.MakeGenericType(
                            resultTypeArguments
                        );
                    }
                }
                else
                {
                    resultType = FindImplementSubclass(type);
                }

                lutInfos[type] = info = new Info()
                {
                    resultType = resultType,
                    scopeMode = DefaultScopeMode,
                };
            }
            return info.resultType;
        }
        public static object Get(Type type)
        {
            FindType(type);
            var info = lutInfos[type];
            if (info.scopeMode == Single && singletons.TryGetValue(type, out var existing))
            {
                return existing;
            }
            var inst = Activator.CreateInstance(info.resultType);
            if (info.scopeMode == Single && inst != null)
            {
                singletons[type] = inst;
            }
            return inst;
        }

        public static T Get<T>()
        {
            var inst = Get(typeof(T));
            Debug.Log($"inst {inst} to {typeof(T)}");
            return (T)Get(typeof(T));
        }

        public static void AddTransient<TContract, TResult>()
        {
            lutInfos[typeof(TContract)] = new()
            {
                resultType = typeof(TResult),
                scopeMode = Transient,
            };
        }

        public static void AddSingle<TContract, TResult>(bool noLazy = false)
        {
            lutInfos[typeof(TContract)] = new()
            {
                resultType = typeof(TResult),
                scopeMode = Single,
            };
            if (noLazy) Get<TContract>();
        }

        public static ServiceScopeMode GetScopeMode<TContract>()
        {
            if (lutInfos.TryGetValue(typeof(TContract), out var info))
            {
                return info.scopeMode;
            }
            return DefaultScopeMode;
        }

    }
    struct Info
    {
        public Type resultType;
        public ServiceScopeMode scopeMode;
    }
}