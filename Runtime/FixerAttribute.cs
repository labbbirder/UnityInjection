using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Scripting;

namespace com.bbbirder.injection
{
    [Preserve, AttributeUsage(AttributeTargets.Method)]
    public class FixerAttribute : InjectionAttribute
    {
        public Type InjectType { get; private set; }
        public string MethodName { get; private set; }
        public string OverwriteName { get; private set; }
        
        public FixerAttribute(
            Type type, string methodName, string overwriteName
        )
        {
            InjectType = type;
            MethodName = methodName;
            OverwriteName = overwriteName;
        }

        public override IEnumerable<InjectionInfo> ProvideInjections()
        {
            yield return new(){
                InjectedMethod = InjectType.GetMember(MethodName, bindingFlags)
                    .OfType<MethodInfo>()
                    .FirstOrDefault(),
                FixingMethod = targetMember as MethodInfo,
                OriginReceiver = f=>{
                    targetType.GetField(OverwriteName, bindingFlags).SetValue(null,f);
                },
            };
        }

        public override void OnReceiveTarget()
        {

        }

        static BindingFlags bindingFlags = 0
            | BindingFlags.Static
            | BindingFlags.Instance
            | BindingFlags.Public
            | BindingFlags.NonPublic
            ;

    }
}