using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using com.bbbirder;

namespace com.bbbirder.unity {
    [AttributeUsage(AttributeTargets.Method)]
    public class FixerAttribute:InjectionAttribute{
        public Type InjectType {get;private set;}
        public string MethodName {get;private set;}
        public string OverwriteName {get;private set;}
        public FixerAttribute(
            Type type,string methodName,string overwriteName
        )
        {
            InjectType = type;
            MethodName = methodName;
            OverwriteName = overwriteName;
        }
        public override void OnReceiveTarget()
        {
            this.InjectedMethod = InjectType.GetMember(MethodName,bindingFlags)
                .OfType<MethodInfo>()
                .FirstOrDefault();
            this.FixingMethod = targetMember as MethodInfo;
            this.OriginSavingField = targetType.GetField(OverwriteName,bindingFlags);
        }
        static BindingFlags bindingFlags = 0
            | BindingFlags.Static
            | BindingFlags.Instance
            | BindingFlags.Public
            | BindingFlags.NonPublic
            ;

    }
}