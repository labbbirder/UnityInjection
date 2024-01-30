using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using com.bbbirder.injection;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Scripting;


namespace com.bbbirder.injection
{
    using static System.Reflection.BindingFlags;
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class SimpleDIAttribute : InjectionAttribute
    {
        static MethodInfo s_MetaMethodInfo;
        static MethodInfo s_StaticMetaMethodInfo;
        static MethodInfo s_miMetaConstructor;
        public SimpleDIAttribute()
        {
        }

        bool IsStatic(MemberInfo memberInfo)
        {
            if (memberInfo is FieldInfo fieldInfo) return fieldInfo.IsStatic;
            if (memberInfo is PropertyInfo propertyInfo)
            {
                if (!propertyInfo.CanRead)
                {
                    return propertyInfo.GetSetMethod(nonPublic: true).IsStatic;
                }
                return propertyInfo.GetGetMethod(nonPublic: true).IsStatic;
            }
            return false;
        }

        [Preserve]
        static TFunc MetaConstructor<TFunc>(Action<object> action, Type instType, bool isStatic)
            where TFunc : Delegate
        {
            var argtypes = typeof(TFunc).GetGenericArguments();
            var act = Expression.Constant(action);
            var args = new List<ParameterExpression>(argtypes.Length + 1);
            // if (!isStatic) args.Add(Expression.Parameter(instType, "inst"));
            for (int i = 0; i < argtypes.Length; i++)
            {
                args.Add(Expression.Parameter(argtypes[i], "arg" + i));
            }
            var ivk = Expression.Call(act,
                typeof(Action<object>).GetMethod("Invoke"),
                isStatic ? Expression.Constant(null) : args[0]);
            var lambda = Expression.Lambda<TFunc>(ivk, args);
            return lambda.Compile();
        }

        ConstructorInfo[] Get_Ctors(Type type)
        {
            return type.GetConstructors(Public | NonPublic | Instance);
        }

        ConstructorInfo[] Get_CCtor(Type type)
        {
            return type.GetConstructors(Public | NonPublic | Static);
        }

        bool CanWrite(MemberInfo memberInfo)
        {
            if (memberInfo is FieldInfo) return true;
            if (memberInfo is PropertyInfo propertyInfo) return propertyInfo.CanWrite;
            return false;
        }
        void SetMemberValue(MemberInfo memberInfo, object inst, object value)
        {
            if (memberInfo is FieldInfo fi)
            {
                fi.SetValue(inst, value);
                return;
            }
            if (memberInfo is PropertyInfo pi)
            {
                pi.SetValue(inst, value);
                return;
            }
        }
        Type GetMemberType(MemberInfo memberInfo)
        {
            if (memberInfo is FieldInfo fi) return fi.FieldType;
            if (memberInfo is PropertyInfo pi) return pi.PropertyType;
            return default;
        }
        public override IEnumerable<InjectionInfo> ProvideInjections()
        {
            s_MetaMethodInfo ??= typeof(SimpleDIAttribute).GetMethod(nameof(MetaGet), Static | NonPublic);
            s_StaticMetaMethodInfo ??= typeof(SimpleDIAttribute).GetMethod(nameof(StaticMetaGet), Static | NonPublic);
            s_miMetaConstructor ??= typeof(SimpleDIAttribute).GetMethod(nameof(MetaConstructor), Static | NonPublic);
            if (targetMember is not PropertyInfo and not FieldInfo)
                throw new Exception($"cannot inject {targetMember} on type {targetType}, only fields and properties allowed");
            var memberType = GetMemberType(targetMember);
            var isStatic = IsStatic(targetMember);
            var canWrite = CanWrite(targetMember);
            // var declaringType = targetMember.DeclaringType;
            if (isStatic)
            {
                if (canWrite)
                {
                    // set on fix instantly
                    yield return InjectionInfo.Create(() =>
                    {
                        SetMemberValue(targetMember, null, GetContainerInst(memberType, targetType));
                    });
                }
                else
                {
                    // inject get method
                    var propertyInfo = targetMember as PropertyInfo;
                    var fixingMethod = s_StaticMetaMethodInfo.MakeGenericMethod(propertyInfo.PropertyType, targetType);
                    yield return InjectionInfo.Create(
                        propertyInfo.GetGetMethod(nonPublic: true),
                        fixingMethod
                    );
                }
            }
            else
            {
                if (canWrite)
                {
                    // inject constructor
                    var constructors = Get_Ctors(targetType);
                    var argtypes = new List<Type>();
                    foreach (var constructor in constructors)
                    {
                        Delegate rawAction = default;
                        argtypes.Clear();
                        argtypes.Add(targetType);
                        foreach (var p in constructor.GetParameters())
                        {
                            argtypes.Add(p.ParameterType);
                        }
                        var miInstAction = default(Type);
                        if (argtypes.Count == 0)
                        {
                            miInstAction = typeof(System.Action);
                        }
                        else
                        {
                            var miGenericAction = Type.GetType("System.Action`" + argtypes.Count);
                            miInstAction = miGenericAction.MakeGenericType(argtypes.ToArray());
                        }
                        var miCtorInst = s_miMetaConstructor.MakeGenericMethod(miInstAction);
                        var fixingFunc = miCtorInst.Invoke(null, new object[]{
                            (Action<object>)fixedContructor,targetType,isStatic
                        }) as Delegate;
                        yield return InjectionInfo.Create(
                            constructor,
                            fixingFunc,
                            f => rawAction = f
                        );
                        void fixedContructor(object inst)
                        {
                            SetMemberValue(targetMember, inst, GetContainerInst(memberType, targetType));
                            rawAction.GetType().GetMethod("Invoke").Invoke(rawAction, new[] { inst });
                        }
                    }
                }
                else
                {
                    // inject get method
                    var propertyInfo = targetMember as PropertyInfo;
                    var fixingMethod = s_MetaMethodInfo.MakeGenericMethod(targetType, propertyInfo.PropertyType, targetType);
                    yield return InjectionInfo.Create(
                        propertyInfo.GetGetMethod(nonPublic: true),
                        fixingMethod
                    );
                }
            }
        }

        #if UNITY_2021_3_OR_NEWER
        [HideInCallstack]
        #endif
        static TRet MetaGet<T, TRet, TDecl>(T _) where T : class where TRet : class
        {
            return GetContainerInst(typeof(TRet), typeof(TDecl)) as TRet;
        }

        #if UNITY_2021_3_OR_NEWER
        [HideInCallstack]
        #endif
        static TRet StaticMetaGet<TRet, TDecl>() where TRet : class
        {
            return GetContainerInst(typeof(TRet), typeof(TDecl)) as TRet;
        }

        #if UNITY_2021_3_OR_NEWER
        [HideInCallstack]
        #endif
        static object GetContainerInst(Type desiredType, Type declaringType)
        {
            return ServiceContainer.Get(desiredType, declaringType, false);
        }
    }
}