using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace com.bbbirder.unity{
    [AttributeUsage(AttributeTargets.Method)]
    public abstract partial class DecoratorAttribute:InjectionAttribute{
        MulticastDelegate originFunc;
        static Type ThisType = typeof(DecoratorAttribute);

        bool? m_IsAsyncMethod = null;
        /// <summary>
        /// Determine target whether an async method
        /// </summary>
        /// <typeparam name="AsyncStateMachineAttribute"></typeparam>
        /// <returns></returns>
        public bool IsAsyncMethod
            => m_IsAsyncMethod ??= targetMember.GetCustomAttribute<AsyncStateMachineAttribute>()!=null;

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

        public struct InvocationInfo<T>{
            internal Func<T> invoker;
            internal Func<T,INotifyCompletion> awaiterGetter;
            internal Func<object[]> argumentGetter;

            /// <summary>
            /// the arguments of current invocation
            /// </summary>
            /// <returns></returns>
            public object[] Arguments=>argumentGetter();

            /// <summary>
            /// the methodInfo of original method
            /// </summary>
            /// <value></value>
            public MethodInfo Method{
                get;set;
            }

            /// <summary>
            /// invoke original method without extra overhead
            /// </summary>
            /// <returns></returns>
            public T FastInvoke(){
                return invoker.Invoke();
            }

            /// <summary>
            /// Get the awaiter when its an async method
            /// </summary>
            /// <param name="result"></param>
            /// <returns></returns>
            public INotifyCompletion GetAwaiter(T result){
                if(result is null) return null;
                if(awaiterGetter is null){
                    var GetMethod = result.GetType().GetMethod("GetAwaiter");
                    if(GetMethod is null) return null;
                    var metaMethod = this.GetType().GetMethod(nameof(MetaGetAwaiter),BindingFlags.NonPublic|BindingFlags.Instance);
                    var metaGetter = metaMethod.MakeGenericMethod(GetMethod.ReturnType).Invoke(this,new []{GetMethod});
                    awaiterGetter = (Func<T,INotifyCompletion>)metaGetter;
                }
                if(awaiterGetter is null) return null;
                return awaiterGetter(result);
            }
            internal Func<T,INotifyCompletion> MetaGetAwaiter<N>(MethodInfo method) where N:INotifyCompletion{
                var func = method.CreateDelegate(typeof(Func<T,N>)) as Func<T,N>;
                return t=>func(t);
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