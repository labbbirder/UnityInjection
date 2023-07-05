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
        public static MethodInfo GetOriginMethodFor(MethodInfo mi){
            var oriName = Settings.GetOriginMethodName(mi.Name);
            return mi.DeclaringType.GetMethod(oriName,bindingFlags);
        }

        static void FixMethod(InjectionAttribute injection){
            var targetMethod = injection.InjectedMethod;
            var fixingMethod = injection.FixingMethod;
            var fixingDelegate = injection.FixingDelegate;
            var originSavingField = injection.OriginSavingField;
            var originSavingTarget = injection.OriginSavingTarget;
            var targetType = targetMethod.DeclaringType;
            var methodName = targetMethod.Name;
            // set static field value
            var sfld = targetType.GetField(Settings.GetInjectedFieldName(methodName),bindingFlags);
            try{
                if(sfld is null) {
                    logError($"Unable to fix target method {methodName} in type {targetType}, this may caused by injection issues." 
                    + " Try execute menu [Tools/bbbirder/Inject for Editor] if you see this in Editor mode");
                }
                var @delegate = fixingDelegate ?? fixingMethod.CreateDelegate(sfld.FieldType);
                if(@delegate is null){
                    logError($"Unable to create delegate for replace method {fixingMethod}, whose target is {methodName}");
                }
                sfld.SetValue(null,@delegate);
            } catch (Exception e){
                var msg = $"error on create and set delegate for injection method {methodName}\n{e.Message}\n{e.StackTrace}";
                logError(msg);
            }

            // set overwrite origin field
            var originMethod = targetType.GetMethod(Settings.GetOriginMethodName(methodName),bindingFlags);
            // if(originSavingField != null){
                try {
                    var oriDelegate = originMethod.CreateDelegate(sfld.FieldType);
                    if(oriDelegate is null){
                        logError($"create original delegate for {methodName} failed");
                    }
                    originSavingField.SetValue(originSavingTarget,oriDelegate);
                } catch (Exception e) {
                    var msg = $"error on create and set delegate for original method {methodName}\n{e.Message}\n{e.StackTrace}";
                    logError(msg);
                }
            // }
            void logError(string message){
                throw new(message);
            }
        }
        /// <summary>
        /// Get all injections in current domain.
        /// </summary>
        /// <param name="assemblies">The assemblies to search in. All loaded assemblies if omitted</param>
        /// <returns></returns>
        public static InjectionAttribute[] GetAllInjections(Assembly[] assemblies=null) {
            assemblies??=AppDomain.CurrentDomain.GetAssemblies();
            var injections =  assemblies
                // .Where(a=>a.MayContainsInjection()) 
                .SelectMany(a=>Retriever.GetAllAttributes<InjectionAttribute>(a))
                .ToArray();
            return injections;
        }
        // public static T GetRawCall<T>(Type t,string methodName) where T:MulticastDelegate{
        //     var key = (t,methodName);
        //     if(!s_rawMethodInfos.TryGetValue(key,out var mi)){
        //         throw new($"there is no injected method: {methodName} on type:{t}");
        //     }
        //     if(!s_rawCalls.TryGetValue(key,out var call)){
        //         call = s_rawCalls[key] = mi.CreateDelegate(typeof(T));
        //     }
        //     return (T)call;
        // }

        // static bool MayContainsInjection(this Assembly assembly) {
        //     checkedAssemblyName ??= typeof(InjectionAttribute).Assembly.GetName().ToString();
        //     return assembly.GetReferencedAssemblies()
        //         .Any(a => a.ToString() == checkedAssemblyName);
        // }

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
                    codeBase = ((!File.Exists("/" + codeBase)) ? Path.GetFullPath(codeBase) : ("/" + codeBase));
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

        static InjectionAttribute[] m_allInjections;
        public static InjectionAttribute[] allInjections=>m_allInjections ??= GetAllInjections();

        static string checkedAssemblyName = null;
        static BindingFlags bindingFlags = 0
            | BindingFlags.Static
            | BindingFlags.Instance
            | BindingFlags.Public
            | BindingFlags.NonPublic
            ;
    }
}