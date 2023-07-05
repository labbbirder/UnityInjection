// Dont Modify It: This is an Auto-Generated File
using System;
using System.Reflection;


namespace com.bbbirder.unity {
    partial class DecoratorAttribute{
        Func<T1,R> UniversalFunc<T1,R>(MethodInfo mi){
            T1 t1 = default;
            var invocation = new InvocationInfo<R>(){
                invoker = ()=>((Func<T1,R>)originFunc).Invoke(t1),
                Method = mi,
                argumentGetter = ()=>new object[]{ t1 }
            };
            return (T1 _t1)=>{
                t1 = _t1;
                return Decorate<R>(invocation);
            };
        }
        Action<T1> UniversalAction<T1>(MethodInfo mi){
            T1 t1 = default;
            var invocation = new InvocationInfo<object>(){
                invoker = ()=>{
                    ((Action<T1>)originFunc).Invoke(t1);
                    return null;
                },
                Method = mi,
                argumentGetter = ()=>new object[]{ t1 }
            };
            return (T1 _t1)=>{
                t1 = _t1;
                Decorate<object>(invocation);
            };
        }
        Func<T1,T2,R> UniversalFunc<T1,T2,R>(MethodInfo mi){
            T1 t1 = default;
            T2 t2 = default;
            var invocation = new InvocationInfo<R>(){
                invoker = ()=>((Func<T1,T2,R>)originFunc).Invoke(t1,t2),
                Method = mi,
                argumentGetter = ()=>new object[]{ t1,t2 }
            };
            return (T1 _t1,T2 _t2)=>{
                t1 = _t1;
                t2 = _t2;
                return Decorate<R>(invocation);
            };
        }
        Action<T1,T2> UniversalAction<T1,T2>(MethodInfo mi){
            T1 t1 = default;
            T2 t2 = default;
            var invocation = new InvocationInfo<object>(){
                invoker = ()=>{
                    ((Action<T1,T2>)originFunc).Invoke(t1,t2);
                    return null;
                },
                Method = mi,
                argumentGetter = ()=>new object[]{ t1,t2 }
            };
            return (T1 _t1,T2 _t2)=>{
                t1 = _t1;
                t2 = _t2;
                Decorate<object>(invocation);
            };
        }
        Func<T1,T2,T3,R> UniversalFunc<T1,T2,T3,R>(MethodInfo mi){
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            var invocation = new InvocationInfo<R>(){
                invoker = ()=>((Func<T1,T2,T3,R>)originFunc).Invoke(t1,t2,t3),
                Method = mi,
                argumentGetter = ()=>new object[]{ t1,t2,t3 }
            };
            return (T1 _t1,T2 _t2,T3 _t3)=>{
                t1 = _t1;
                t2 = _t2;
                t3 = _t3;
                return Decorate<R>(invocation);
            };
        }
        Action<T1,T2,T3> UniversalAction<T1,T2,T3>(MethodInfo mi){
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            var invocation = new InvocationInfo<object>(){
                invoker = ()=>{
                    ((Action<T1,T2,T3>)originFunc).Invoke(t1,t2,t3);
                    return null;
                },
                Method = mi,
                argumentGetter = ()=>new object[]{ t1,t2,t3 }
            };
            return (T1 _t1,T2 _t2,T3 _t3)=>{
                t1 = _t1;
                t2 = _t2;
                t3 = _t3;
                Decorate<object>(invocation);
            };
        }
        Func<T1,T2,T3,T4,R> UniversalFunc<T1,T2,T3,T4,R>(MethodInfo mi){
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            var invocation = new InvocationInfo<R>(){
                invoker = ()=>((Func<T1,T2,T3,T4,R>)originFunc).Invoke(t1,t2,t3,t4),
                Method = mi,
                argumentGetter = ()=>new object[]{ t1,t2,t3,t4 }
            };
            return (T1 _t1,T2 _t2,T3 _t3,T4 _t4)=>{
                t1 = _t1;
                t2 = _t2;
                t3 = _t3;
                t4 = _t4;
                return Decorate<R>(invocation);
            };
        }
        Action<T1,T2,T3,T4> UniversalAction<T1,T2,T3,T4>(MethodInfo mi){
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            var invocation = new InvocationInfo<object>(){
                invoker = ()=>{
                    ((Action<T1,T2,T3,T4>)originFunc).Invoke(t1,t2,t3,t4);
                    return null;
                },
                Method = mi,
                argumentGetter = ()=>new object[]{ t1,t2,t3,t4 }
            };
            return (T1 _t1,T2 _t2,T3 _t3,T4 _t4)=>{
                t1 = _t1;
                t2 = _t2;
                t3 = _t3;
                t4 = _t4;
                Decorate<object>(invocation);
            };
        }
        Func<T1,T2,T3,T4,T5,R> UniversalFunc<T1,T2,T3,T4,T5,R>(MethodInfo mi){
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            T5 t5 = default;
            var invocation = new InvocationInfo<R>(){
                invoker = ()=>((Func<T1,T2,T3,T4,T5,R>)originFunc).Invoke(t1,t2,t3,t4,t5),
                Method = mi,
                argumentGetter = ()=>new object[]{ t1,t2,t3,t4,t5 }
            };
            return (T1 _t1,T2 _t2,T3 _t3,T4 _t4,T5 _t5)=>{
                t1 = _t1;
                t2 = _t2;
                t3 = _t3;
                t4 = _t4;
                t5 = _t5;
                return Decorate<R>(invocation);
            };
        }
        Action<T1,T2,T3,T4,T5> UniversalAction<T1,T2,T3,T4,T5>(MethodInfo mi){
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            T5 t5 = default;
            var invocation = new InvocationInfo<object>(){
                invoker = ()=>{
                    ((Action<T1,T2,T3,T4,T5>)originFunc).Invoke(t1,t2,t3,t4,t5);
                    return null;
                },
                Method = mi,
                argumentGetter = ()=>new object[]{ t1,t2,t3,t4,t5 }
            };
            return (T1 _t1,T2 _t2,T3 _t3,T4 _t4,T5 _t5)=>{
                t1 = _t1;
                t2 = _t2;
                t3 = _t3;
                t4 = _t4;
                t5 = _t5;
                Decorate<object>(invocation);
            };
        }
        Func<T1,T2,T3,T4,T5,T6,R> UniversalFunc<T1,T2,T3,T4,T5,T6,R>(MethodInfo mi){
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            T5 t5 = default;
            T6 t6 = default;
            var invocation = new InvocationInfo<R>(){
                invoker = ()=>((Func<T1,T2,T3,T4,T5,T6,R>)originFunc).Invoke(t1,t2,t3,t4,t5,t6),
                Method = mi,
                argumentGetter = ()=>new object[]{ t1,t2,t3,t4,t5,t6 }
            };
            return (T1 _t1,T2 _t2,T3 _t3,T4 _t4,T5 _t5,T6 _t6)=>{
                t1 = _t1;
                t2 = _t2;
                t3 = _t3;
                t4 = _t4;
                t5 = _t5;
                t6 = _t6;
                return Decorate<R>(invocation);
            };
        }
        Action<T1,T2,T3,T4,T5,T6> UniversalAction<T1,T2,T3,T4,T5,T6>(MethodInfo mi){
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            T5 t5 = default;
            T6 t6 = default;
            var invocation = new InvocationInfo<object>(){
                invoker = ()=>{
                    ((Action<T1,T2,T3,T4,T5,T6>)originFunc).Invoke(t1,t2,t3,t4,t5,t6);
                    return null;
                },
                Method = mi,
                argumentGetter = ()=>new object[]{ t1,t2,t3,t4,t5,t6 }
            };
            return (T1 _t1,T2 _t2,T3 _t3,T4 _t4,T5 _t5,T6 _t6)=>{
                t1 = _t1;
                t2 = _t2;
                t3 = _t3;
                t4 = _t4;
                t5 = _t5;
                t6 = _t6;
                Decorate<object>(invocation);
            };
        }
        Func<T1,T2,T3,T4,T5,T6,T7,R> UniversalFunc<T1,T2,T3,T4,T5,T6,T7,R>(MethodInfo mi){
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            T5 t5 = default;
            T6 t6 = default;
            T7 t7 = default;
            var invocation = new InvocationInfo<R>(){
                invoker = ()=>((Func<T1,T2,T3,T4,T5,T6,T7,R>)originFunc).Invoke(t1,t2,t3,t4,t5,t6,t7),
                Method = mi,
                argumentGetter = ()=>new object[]{ t1,t2,t3,t4,t5,t6,t7 }
            };
            return (T1 _t1,T2 _t2,T3 _t3,T4 _t4,T5 _t5,T6 _t6,T7 _t7)=>{
                t1 = _t1;
                t2 = _t2;
                t3 = _t3;
                t4 = _t4;
                t5 = _t5;
                t6 = _t6;
                t7 = _t7;
                return Decorate<R>(invocation);
            };
        }
        Action<T1,T2,T3,T4,T5,T6,T7> UniversalAction<T1,T2,T3,T4,T5,T6,T7>(MethodInfo mi){
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            T5 t5 = default;
            T6 t6 = default;
            T7 t7 = default;
            var invocation = new InvocationInfo<object>(){
                invoker = ()=>{
                    ((Action<T1,T2,T3,T4,T5,T6,T7>)originFunc).Invoke(t1,t2,t3,t4,t5,t6,t7);
                    return null;
                },
                Method = mi,
                argumentGetter = ()=>new object[]{ t1,t2,t3,t4,t5,t6,t7 }
            };
            return (T1 _t1,T2 _t2,T3 _t3,T4 _t4,T5 _t5,T6 _t6,T7 _t7)=>{
                t1 = _t1;
                t2 = _t2;
                t3 = _t3;
                t4 = _t4;
                t5 = _t5;
                t6 = _t6;
                t7 = _t7;
                Decorate<object>(invocation);
            };
        }
        Func<T1,T2,T3,T4,T5,T6,T7,T8,R> UniversalFunc<T1,T2,T3,T4,T5,T6,T7,T8,R>(MethodInfo mi){
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            T5 t5 = default;
            T6 t6 = default;
            T7 t7 = default;
            T8 t8 = default;
            var invocation = new InvocationInfo<R>(){
                invoker = ()=>((Func<T1,T2,T3,T4,T5,T6,T7,T8,R>)originFunc).Invoke(t1,t2,t3,t4,t5,t6,t7,t8),
                Method = mi,
                argumentGetter = ()=>new object[]{ t1,t2,t3,t4,t5,t6,t7,t8 }
            };
            return (T1 _t1,T2 _t2,T3 _t3,T4 _t4,T5 _t5,T6 _t6,T7 _t7,T8 _t8)=>{
                t1 = _t1;
                t2 = _t2;
                t3 = _t3;
                t4 = _t4;
                t5 = _t5;
                t6 = _t6;
                t7 = _t7;
                t8 = _t8;
                return Decorate<R>(invocation);
            };
        }
        Action<T1,T2,T3,T4,T5,T6,T7,T8> UniversalAction<T1,T2,T3,T4,T5,T6,T7,T8>(MethodInfo mi){
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            T5 t5 = default;
            T6 t6 = default;
            T7 t7 = default;
            T8 t8 = default;
            var invocation = new InvocationInfo<object>(){
                invoker = ()=>{
                    ((Action<T1,T2,T3,T4,T5,T6,T7,T8>)originFunc).Invoke(t1,t2,t3,t4,t5,t6,t7,t8);
                    return null;
                },
                Method = mi,
                argumentGetter = ()=>new object[]{ t1,t2,t3,t4,t5,t6,t7,t8 }
            };
            return (T1 _t1,T2 _t2,T3 _t3,T4 _t4,T5 _t5,T6 _t6,T7 _t7,T8 _t8)=>{
                t1 = _t1;
                t2 = _t2;
                t3 = _t3;
                t4 = _t4;
                t5 = _t5;
                t6 = _t6;
                t7 = _t7;
                t8 = _t8;
                Decorate<object>(invocation);
            };
        }
        Func<T1,T2,T3,T4,T5,T6,T7,T8,T9,R> UniversalFunc<T1,T2,T3,T4,T5,T6,T7,T8,T9,R>(MethodInfo mi){
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            T5 t5 = default;
            T6 t6 = default;
            T7 t7 = default;
            T8 t8 = default;
            T9 t9 = default;
            var invocation = new InvocationInfo<R>(){
                invoker = ()=>((Func<T1,T2,T3,T4,T5,T6,T7,T8,T9,R>)originFunc).Invoke(t1,t2,t3,t4,t5,t6,t7,t8,t9),
                Method = mi,
                argumentGetter = ()=>new object[]{ t1,t2,t3,t4,t5,t6,t7,t8,t9 }
            };
            return (T1 _t1,T2 _t2,T3 _t3,T4 _t4,T5 _t5,T6 _t6,T7 _t7,T8 _t8,T9 _t9)=>{
                t1 = _t1;
                t2 = _t2;
                t3 = _t3;
                t4 = _t4;
                t5 = _t5;
                t6 = _t6;
                t7 = _t7;
                t8 = _t8;
                t9 = _t9;
                return Decorate<R>(invocation);
            };
        }
        Action<T1,T2,T3,T4,T5,T6,T7,T8,T9> UniversalAction<T1,T2,T3,T4,T5,T6,T7,T8,T9>(MethodInfo mi){
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            T5 t5 = default;
            T6 t6 = default;
            T7 t7 = default;
            T8 t8 = default;
            T9 t9 = default;
            var invocation = new InvocationInfo<object>(){
                invoker = ()=>{
                    ((Action<T1,T2,T3,T4,T5,T6,T7,T8,T9>)originFunc).Invoke(t1,t2,t3,t4,t5,t6,t7,t8,t9);
                    return null;
                },
                Method = mi,
                argumentGetter = ()=>new object[]{ t1,t2,t3,t4,t5,t6,t7,t8,t9 }
            };
            return (T1 _t1,T2 _t2,T3 _t3,T4 _t4,T5 _t5,T6 _t6,T7 _t7,T8 _t8,T9 _t9)=>{
                t1 = _t1;
                t2 = _t2;
                t3 = _t3;
                t4 = _t4;
                t5 = _t5;
                t6 = _t6;
                t7 = _t7;
                t8 = _t8;
                t9 = _t9;
                Decorate<object>(invocation);
            };
        }
        Func<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,R> UniversalFunc<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,R>(MethodInfo mi){
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            T5 t5 = default;
            T6 t6 = default;
            T7 t7 = default;
            T8 t8 = default;
            T9 t9 = default;
            T10 t10 = default;
            var invocation = new InvocationInfo<R>(){
                invoker = ()=>((Func<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,R>)originFunc).Invoke(t1,t2,t3,t4,t5,t6,t7,t8,t9,t10),
                Method = mi,
                argumentGetter = ()=>new object[]{ t1,t2,t3,t4,t5,t6,t7,t8,t9,t10 }
            };
            return (T1 _t1,T2 _t2,T3 _t3,T4 _t4,T5 _t5,T6 _t6,T7 _t7,T8 _t8,T9 _t9,T10 _t10)=>{
                t1 = _t1;
                t2 = _t2;
                t3 = _t3;
                t4 = _t4;
                t5 = _t5;
                t6 = _t6;
                t7 = _t7;
                t8 = _t8;
                t9 = _t9;
                t10 = _t10;
                return Decorate<R>(invocation);
            };
        }
        Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10> UniversalAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>(MethodInfo mi){
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            T5 t5 = default;
            T6 t6 = default;
            T7 t7 = default;
            T8 t8 = default;
            T9 t9 = default;
            T10 t10 = default;
            var invocation = new InvocationInfo<object>(){
                invoker = ()=>{
                    ((Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>)originFunc).Invoke(t1,t2,t3,t4,t5,t6,t7,t8,t9,t10);
                    return null;
                },
                Method = mi,
                argumentGetter = ()=>new object[]{ t1,t2,t3,t4,t5,t6,t7,t8,t9,t10 }
            };
            return (T1 _t1,T2 _t2,T3 _t3,T4 _t4,T5 _t5,T6 _t6,T7 _t7,T8 _t8,T9 _t9,T10 _t10)=>{
                t1 = _t1;
                t2 = _t2;
                t3 = _t3;
                t4 = _t4;
                t5 = _t5;
                t6 = _t6;
                t7 = _t7;
                t8 = _t8;
                t9 = _t9;
                t10 = _t10;
                Decorate<object>(invocation);
            };
        }
        Func<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,R> UniversalFunc<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,R>(MethodInfo mi){
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            T5 t5 = default;
            T6 t6 = default;
            T7 t7 = default;
            T8 t8 = default;
            T9 t9 = default;
            T10 t10 = default;
            T11 t11 = default;
            var invocation = new InvocationInfo<R>(){
                invoker = ()=>((Func<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,R>)originFunc).Invoke(t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11),
                Method = mi,
                argumentGetter = ()=>new object[]{ t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11 }
            };
            return (T1 _t1,T2 _t2,T3 _t3,T4 _t4,T5 _t5,T6 _t6,T7 _t7,T8 _t8,T9 _t9,T10 _t10,T11 _t11)=>{
                t1 = _t1;
                t2 = _t2;
                t3 = _t3;
                t4 = _t4;
                t5 = _t5;
                t6 = _t6;
                t7 = _t7;
                t8 = _t8;
                t9 = _t9;
                t10 = _t10;
                t11 = _t11;
                return Decorate<R>(invocation);
            };
        }
        Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11> UniversalAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>(MethodInfo mi){
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            T5 t5 = default;
            T6 t6 = default;
            T7 t7 = default;
            T8 t8 = default;
            T9 t9 = default;
            T10 t10 = default;
            T11 t11 = default;
            var invocation = new InvocationInfo<object>(){
                invoker = ()=>{
                    ((Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>)originFunc).Invoke(t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11);
                    return null;
                },
                Method = mi,
                argumentGetter = ()=>new object[]{ t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11 }
            };
            return (T1 _t1,T2 _t2,T3 _t3,T4 _t4,T5 _t5,T6 _t6,T7 _t7,T8 _t8,T9 _t9,T10 _t10,T11 _t11)=>{
                t1 = _t1;
                t2 = _t2;
                t3 = _t3;
                t4 = _t4;
                t5 = _t5;
                t6 = _t6;
                t7 = _t7;
                t8 = _t8;
                t9 = _t9;
                t10 = _t10;
                t11 = _t11;
                Decorate<object>(invocation);
            };
        }
        Func<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,R> UniversalFunc<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,R>(MethodInfo mi){
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            T5 t5 = default;
            T6 t6 = default;
            T7 t7 = default;
            T8 t8 = default;
            T9 t9 = default;
            T10 t10 = default;
            T11 t11 = default;
            T12 t12 = default;
            var invocation = new InvocationInfo<R>(){
                invoker = ()=>((Func<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,R>)originFunc).Invoke(t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11,t12),
                Method = mi,
                argumentGetter = ()=>new object[]{ t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11,t12 }
            };
            return (T1 _t1,T2 _t2,T3 _t3,T4 _t4,T5 _t5,T6 _t6,T7 _t7,T8 _t8,T9 _t9,T10 _t10,T11 _t11,T12 _t12)=>{
                t1 = _t1;
                t2 = _t2;
                t3 = _t3;
                t4 = _t4;
                t5 = _t5;
                t6 = _t6;
                t7 = _t7;
                t8 = _t8;
                t9 = _t9;
                t10 = _t10;
                t11 = _t11;
                t12 = _t12;
                return Decorate<R>(invocation);
            };
        }
        Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12> UniversalAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>(MethodInfo mi){
            T1 t1 = default;
            T2 t2 = default;
            T3 t3 = default;
            T4 t4 = default;
            T5 t5 = default;
            T6 t6 = default;
            T7 t7 = default;
            T8 t8 = default;
            T9 t9 = default;
            T10 t10 = default;
            T11 t11 = default;
            T12 t12 = default;
            var invocation = new InvocationInfo<object>(){
                invoker = ()=>{
                    ((Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>)originFunc).Invoke(t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11,t12);
                    return null;
                },
                Method = mi,
                argumentGetter = ()=>new object[]{ t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11,t12 }
            };
            return (T1 _t1,T2 _t2,T3 _t3,T4 _t4,T5 _t5,T6 _t6,T7 _t7,T8 _t8,T9 _t9,T10 _t10,T11 _t11,T12 _t12)=>{
                t1 = _t1;
                t2 = _t2;
                t3 = _t3;
                t4 = _t4;
                t5 = _t5;
                t6 = _t6;
                t7 = _t7;
                t8 = _t8;
                t9 = _t9;
                t10 = _t10;
                t11 = _t11;
                t12 = _t12;
                Decorate<object>(invocation);
            };
        }
    }
}