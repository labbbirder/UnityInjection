using System.Reflection;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace com.bbbirder.unity
{
    [InheritRetrieve]
    public interface IProxyData : IInjection
    {

    }
    public abstract class ProxyData : IProxyData
    {
        static readonly BindingFlags bindingFlags = 0
            | BindingFlags.Instance
            | BindingFlags.NonPublic
            | BindingFlags.Public
            | BindingFlags.DeclaredOnly
            ;
        protected Dictionary<string, Delegate> getters = new();
        protected Dictionary<string, Delegate> setters = new();
        public Action<string> OnSetProperty { get; set; }
        public Action<string> OnGetProperty { get; set; }
        Func<C, T> ProxyGet<C, T>(string name) where C : ProxyData
        {
            return o =>
            {
                // Debug.Log("get " + name);
                o.OnGetProperty?.Invoke(name);
                var getter = o.getters[name];
                var method = getter as Func<C, T>;
                return method.Invoke(o);
            };
        }
        Action<C, T> ProxySet<C, T>(string name) where C : ProxyData
        {
            return (o, v) =>
            {
                var setter = o.setters[name];
                var method = setter as Action<C, T>;
                method.Invoke(o, v);
                // Debug.Log("set " + name + "=" + v);
                o.OnSetProperty?.Invoke(name);
            };
        }
        public IEnumerable<InjectionInfo> ProvideInjections()
        {
            var targetType = this.GetType();
            if (targetType.IsAbstract || targetType.IsInterface)
                yield break;

            var proxyGet = typeof(ProxyData).GetMethod(nameof(ProxyGet), bindingFlags);
            var proxySet = typeof(ProxyData).GetMethod(nameof(ProxySet), bindingFlags);
            var instMethod = default(MethodInfo);
            var nameArgs = new string[1];
            foreach (var property in targetType.GetProperties(bindingFlags))
            {
                var name = property.Name;
                nameArgs[0] = name;

                instMethod = proxyGet.MakeGenericMethod(targetType, property.PropertyType);
                if (property.GetMethod != null)
                {
                    yield return new()
                    {
                        InjectedMethod = property.GetMethod,
                        FixingDelegate = instMethod.Invoke(this, nameArgs) as Delegate,
                        OriginReceiver = f =>
                        {
                            getters[name] = f;
                        }
                    };
                }
                instMethod = proxySet.MakeGenericMethod(targetType, property.PropertyType);
                if (property.SetMethod != null)
                {
                    yield return new()
                    {
                        InjectedMethod = property.SetMethod,
                        FixingDelegate = instMethod.Invoke(this, nameArgs) as Delegate,
                        OriginReceiver = f =>
                        {
                            setters[name] = f;
                        }
                    };
                }
            }

        }
    }
}