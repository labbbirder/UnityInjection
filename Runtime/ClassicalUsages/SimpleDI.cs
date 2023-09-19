using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace com.bbbirder.injection
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SimpleDIAttribute : InjectionAttribute
    {
        bool isSingle;
        static MethodInfo s_MetaMethodInfo;
        public SimpleDIAttribute(bool isSingle = true){
            this.isSingle = isSingle;
        }
        public override IEnumerable<InjectionInfo> ProvideInjections()
        {
            s_MetaMethodInfo??= typeof(SimpleDIAttribute).GetMethod(nameof(MetaGet),BindingFlags.Static|BindingFlags.NonPublic);
            var propInfo = targetMember as PropertyInfo;
            
            yield return InjectionInfo.Create(
                propInfo.GetMethod,
                s_MetaMethodInfo.MakeGenericMethod(propInfo.PropertyType)
            );
        }
        static T MetaGet<T>(){
            return DefaultDependencyContainer.Get<T>();
        }
    }

    public interface IDependencyContainer
    {
        public void AddRule<T>(Func<T> getter);
        public T Get<T>();
        public object Get(Type type);
    }

    public static class DefaultDependencyContainer
    {
        static Dictionary<Type, Func<object>> getters = new();
        static Dictionary<Type,object> instances = new();

        static object GetByDefault(Type type)
        {
            if(!instances.TryGetValue(type,out var inst)){
                var subtypes = Retriever.GetAllSubtypes(type);
                if (subtypes.Length == 0)
                {
                    throw new ArgumentException($"type {type} doesn't has an implement");
                }
                if (subtypes.Length > 1)
                {
                    Debug.LogWarning($"type {type} exists more than one implements");
                }
                var targetType = subtypes[0];
                instances[type] = inst = Activator.CreateInstance(targetType);
            }
            return inst;
        }

        static public void AddRule<T>(Func<T> getter)
        {
            getters[typeof(T)] = () => getter();
        }

        static public T Get<T>()
        {
            return (T)Get(typeof(T));
        }

        static public object Get(Type type)
        {
            if (getters.TryGetValue(type, out var getter))
            {
                return getter();
            }
            else
            {
                return GetByDefault(type);
            }
        }
    }
}