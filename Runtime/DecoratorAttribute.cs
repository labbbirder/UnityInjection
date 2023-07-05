using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace com.bbbirder.unity{
    
    public abstract partial class DecoratorAttribute:InjectionAttribute{
        MulticastDelegate originFunc;
        static Type ThisType = typeof(DecoratorAttribute);
        public override void OnReceiveTarget(){
            InjectedMethod = this.targetMember as MethodInfo;
            OriginSavingField = ThisType.GetField(nameof(originFunc),BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static);
            OriginSavingTarget = this;

            var originMethod = FixHelper.GetOriginMethodFor(InjectedMethod);
            var HasReturn = InjectedMethod.ReturnType!=typeof(void);
            var methodName = HasReturn ? nameof(UniversalFunc) : nameof(UniversalAction);
            
            // get generic parameter count
            var GenericArgumentCount = InjectedMethod.GetParameters().Length;
            if(!InjectedMethod.IsStatic) GenericArgumentCount++;
            if(HasReturn) GenericArgumentCount++;
            
            // retrieve universal method
            var universalMethod = ThisType.GetMethod(
                methodName,
                GenericArgumentCount,
                BindingFlags.NonPublic|BindingFlags.Instance,
                null,
                new Type[]{typeof(MethodInfo)},
                null
            );
            if(universalMethod is null){
                Debug.LogException(new($"cannot find ${methodName} with generic ${GenericArgumentCount}"));
                return;
            }

            // get generic instance paramters
            var Parameters = InjectedMethod.GetParameters()
                .Select(p=>p.ParameterType)
                .ToList();
            if(!InjectedMethod.IsStatic) Parameters.Insert(0,InjectedMethod.DeclaringType);
            if(HasReturn) Parameters.Add(InjectedMethod.ReturnType);

            // get inner fixing function
            var fixingFunc = universalMethod.MakeGenericMethod(Parameters.ToArray())
            .Invoke(this,new object[] {originMethod}) as Delegate;

            FixingDelegate = fixingFunc;
        }

        public class InvocationInfo<T>{
            internal Func<T> invoker;
            internal Func<object[]> argumentGetter;
            public object[] Arguments=>argumentGetter();
            public MethodInfo Method{
                get;set;
            }
            public T FastInvoke(){
                return invoker.Invoke();
            }
        }

        
        Func<R> UniversalFunc<R>(MethodInfo mi){
            var invocation = new InvocationInfo<R>(){
                invoker = ()=>((Func<R>)originFunc).Invoke(),
                Method = mi,
                argumentGetter = ()=>new object[]{  }
            };
            return ()=>{
                return Decorate<R>(invocation);
            };
        }
        Action UniversalAction(MethodInfo mi,Func<InvocationInfo<object>,object> method){
            var invocation = new InvocationInfo<object>(){
                invoker = ()=>{
                    ((Action)originFunc).Invoke();
                    return null;
                },
                Method = mi,
                argumentGetter = ()=>new object[]{  }
            };
            return ()=>{
                Decorate<object>(invocation);
            };
        }
        
        protected abstract R Decorate<R>(InvocationInfo<R> invocation);
    }

}