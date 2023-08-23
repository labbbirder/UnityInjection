using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using UnityEngine;
using com.bbbirder;

namespace com.bbbirder.unity {
    public static class FixHelper {
        public static void Install() {
            foreach(var injection in allInjections){
                FixMethod(injection);
            }
            Debug.Log($"fixed {allInjections.Length} injections successfully!");
        }
        /// <summary>
        /// check if the assembly of target type is injected
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsInjected(Type type){
            var mark = type.Assembly.GetType($"{Settings.InjectedMarkNamespace}.{Settings.InjectedMarkName}");
            return mark != null;
        }
        public static MethodInfo GetOriginMethodFor(MethodInfo mi){
            var oriName = Settings.GetOriginMethodName(mi.Name);
            return mi.DeclaringType.GetMethod(oriName,bindingFlags);
        }

        static void FixMethod(InjectionInfo injection){
            var targetMethod = injection.InjectedMethod;
            var fixingMethod = injection.FixingMethod;
            var fixingDelegate = injection.FixingDelegate;
            // var originSavingField = injection.OriginSavingField;
            // var originSavingTarget = injection.OriginSavingTarget;
            var targetType = targetMethod.DeclaringType;
            var methodName = targetMethod.Name;
            // set static field value
            var sfld = targetType.GetField(Settings.GetInjectedFieldName(methodName),bindingFlags);
            try{
                if(sfld is null) {
                    throw new($"Unable to fix target method {methodName} in type {targetType}, this may caused by injection issues." 
                    + " Try execute menu [Tools/bbbirder/Inject for Editor] if you see this in Editor mode");
                }
                var @delegate = fixingDelegate ?? fixingMethod.CreateDelegate(sfld.FieldType);
                if(@delegate is null){
                    throw new($"Unable to create delegate for replace method {fixingMethod}, whose target is {methodName}");
                }
                sfld.SetValue(null,@delegate);
            } catch (Exception e){
                var msg = $"error on create and set delegate for injection method {methodName}\n{e.Message}\n{e.StackTrace}";
                Debug.LogError(msg);
                throw;
            }

            // set overwrite origin field
            var originMethod = targetType.GetMethod(Settings.GetOriginMethodName(methodName),bindingFlags);
            // if(originSavingField != null){
                try {
                    var oriDelegate = originMethod.CreateDelegate(sfld.FieldType);
                    if(oriDelegate is null){
                        throw new($"create original delegate for {methodName} failed");
                    }
                    injection.OriginReceiver?.Invoke(oriDelegate);
                    // originSavingField.SetValue(originSavingTarget,oriDelegate);
                } catch (Exception e) {
                    var msg = $"error on create and set delegate for original method {methodName}\n{e.Message}\n{e.StackTrace}";
                    Debug.LogError(msg);
                    throw;
                }
            // }
            // void logError(string message){
            //     throw new(message);
            // }
        }
        /// <summary>
        /// Get all injections in current domain.
        /// </summary>
        /// <param name="assemblies">The assemblies to search in. All loaded assemblies if omitted</param>
        /// <returns></returns>
        public static InjectionInfo[] GetAllInjections(Assembly[] assemblies=null) {
            assemblies??=AppDomain.CurrentDomain.GetAssemblies();
            var injections =  assemblies
                // .Where(a=>a.MayContainsInjection()) 
                .SelectMany(a=>Retriever.GetAllAttributes<InjectionAttribute>(a))
                .SelectMany(attr=>attr.ProvideInjections())
                .ToArray();
            var injections2 = assemblies
                .SelectMany(a=>Retriever.GetAllSubtypes<IInjection>(a))
                .Where(type=>!type.IsInterface && !type.IsAbstract)
                .Select(type=>System.Activator.CreateInstance(type) as IInjection)
                .SelectMany(ii=>ii.ProvideInjections())
                .ToArray();
            injections = injections.Concat(injections2).ToArray();
            return injections;
        }

        public static string GetAssemblyPath(this Assembly assembly)
        {
            if (assembly == null)
            {
                return null;
            }
            if (assembly.IsDynamic)
            {
                return null;
            }
            if (assembly.CodeBase == null)
            {
                return null;
            }
            string text = "file:///";
            string codeBase = assembly.CodeBase;
            if (codeBase.StartsWith(text, StringComparison.InvariantCultureIgnoreCase))
            {
                codeBase = codeBase.Substring(text.Length);
                codeBase = codeBase.Replace('\\', '/');
                if (File.Exists(codeBase))
                {
                    return codeBase;
                }
                if (!Path.IsPathRooted(codeBase))
                {
                    codeBase = (!File.Exists("/" + codeBase)) ? Path.GetFullPath(codeBase) : ("/" + codeBase);
                }
                if (File.Exists(codeBase))
                {
                    return codeBase;
                }
                try
                {
                    codeBase = assembly.Location;
                }
                catch
                {
                    return null;
                }
                if (File.Exists(codeBase))
                {
                    return codeBase;
                }
            }
            if (File.Exists(assembly.Location))
            {
                return assembly.Location;
            }
            return null;
        }

        static InjectionInfo[] m_allInjections;
        public static InjectionInfo[] allInjections=>m_allInjections ??= GetAllInjections();

        // static string checkedAssemblyName = null;
        static BindingFlags bindingFlags = 0
            | BindingFlags.Static
            | BindingFlags.Instance
            | BindingFlags.Public
            | BindingFlags.NonPublic
            ;
    }
}