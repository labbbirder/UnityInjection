using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using UnityEngine;
using System.Runtime.InteropServices;
using com.bbbirder;

namespace com.bbbirder.unity {
    public static class FixHelper {
        public static void Install() {
            foreach(var injection in allInjections){
                var (type,methodName,owName,replaceMethod) = injection;

                // set static field value
                try{
                    var sfld = type.GetField(Settings.GetInjectedFieldName(methodName),bindingFlags);
                    if(sfld is null) {
                        logError($"Unable to fix target method {methodName} in type {type}, this may caused by injection issues." 
                        + " Try execute menu [Tools/bbbirder/Inject for Editor] if you see this in Editor mode");
                    }
                    var @delegate = replaceMethod.CreateDelegate(sfld.FieldType);
                    if(@delegate is null){
                        logError($"Unable to create delegate for replace method {replaceMethod}, whose target is {methodName}");
                    }
                    sfld.SetValue(null,@delegate);
                } catch (Exception e){
                    var msg = $"error on create and set delegate for injection method {methodName}\n{e.Message}\n{e.StackTrace}";
                    logError(msg);
                }

                // set overwrite origin field
                var miOri = type.GetMethod(Settings.GetOriginMethodName(methodName),bindingFlags);
                var fiOri = replaceMethod.DeclaringType?.GetField(owName,bindingFlags);
                if(fiOri is null){
                    logError($"cannot find the field to store original method for method {methodName}");
                }
                try {
                    
                    var oriDelegate = miOri.CreateDelegate(fiOri.FieldType);
                    if(oriDelegate is null){
                        logError($"create original delegate for {methodName} failed");
                    }
                    fiOri.SetValue(null,oriDelegate);
                } catch (Exception e) {
                    var msg = $"error on create and set delegate for original method {methodName}\n{e.Message}\n{e.StackTrace}";
                    logError(msg);
                }
                void logError(string message){
                    var (path,line) = injection.codeLocation;
                    throw new( $"{message} \nInjection (at {path}:{line})");
                }
            }
            Debug.Log($"fixed {allInjections.Length} injections successfully!");
        }

        /// <summary>
        /// Get all injections in current domain.
        /// </summary>
        /// <param name="assemblies">The assemblies to search in. All loaded assemblies if omitted</param>
        /// <returns></returns>
        public static InjectionParams[] GetAllInjections(Assembly[] assemblies=null) {
            assemblies??=AppDomain.CurrentDomain.GetAssemblies();
            var injections =  assemblies
                // .Where(a=>a.MayContainsInjection()) 
                .SelectMany(a=>Retriever.GetAllAttributes<InjectionAttribute>(a))
                .Where(ia=>ia.memberInfo is MethodInfo)
                .Select(ia=>{
                    var m = ia.memberInfo as MethodInfo;
                    var methodName = ia.methodName;
                    if(string.IsNullOrEmpty(methodName)){
                        methodName = m.Name;
                    }
                    return new InjectionParams(){
                        targetType = ia.type,
                        methodName = methodName,
                        overwriteName = ia.overwriteName,
                        replaceMethod = m,
                        codeLocation = ia.codeLocation,
                    };
                })
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
        static bool MayContainsInjection(this Assembly assembly) {
            checkedAssemblyName ??= typeof(InjectionAttribute).Assembly.GetName().ToString();
            return assembly.GetReferencedAssemblies()
                .Any(a => a.ToString() == checkedAssemblyName);
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

        static InjectionParams[] m_allInjections;
        public static InjectionParams[] allInjections=>m_allInjections ??= GetAllInjections();

        // internal static Dictionary<(Type,string),Delegate> s_rawCalls = new ();
        // internal static Dictionary<(Type,string),MethodInfo> s_rawMethodInfos = new ();
        static string checkedAssemblyName = null;
        static BindingFlags bindingFlags = 0
            | BindingFlags.Static
            | BindingFlags.Instance
            | BindingFlags.Public
            | BindingFlags.NonPublic
            ;


        public class InjectionParams {
            public Type targetType;
            public string methodName;
            public string overwriteName;
            public MethodInfo replaceMethod;
            public (string,int) codeLocation;
            public void Deconstruct(out Type type,out string name,out string owName,out MethodInfo method){
                type = targetType;
                name = methodName;
                owName = overwriteName;
                method = replaceMethod;
            }
        }
    }
}