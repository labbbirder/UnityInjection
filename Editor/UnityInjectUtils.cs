using System;
using System.Reflection;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Compilation;
using UnityEditor.Callbacks;
using com.bbbirder.unity;
using Assembly = System.Reflection.Assembly;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace com.bbbirder.unityeditor {
    public static class UnityInjectUtils{
        const string BACKUP_EXT = ".backup";
        
        [MenuItem("Tools/bbbirder/Inject for Editor")]
        static void Inject(){
            InjectEditor(AppDomain.CurrentDomain.GetAssemblies());
        }
        [InitializeOnLoadMethod]
        static void Install() {
            var previousCompiledAssemblies = new List<string>();
            CompilationPipeline.compilationStarted += (o)=>{
                var settings = GetScriptAssemblySettings();
                if(settings is null){
                    Debug.LogError("cannot get BeeScriptCompilationState, Unity Version"+Application.unityVersion);
                    return;
                }
                var BuildingForEditor = (bool)settings?.GetMemberValue("BuildingForEditor",true);
                var OutputDirectory = (string)settings?.GetMemberValue("OutputDirectory",true);
                if(!BuildingForEditor){
                    runtimeOutputDirectory = OutputDirectory;
                }
            };
            //TODO: write to previous compiled asset
            CompilationPipeline.assemblyCompilationFinished += (path,msg)=>{
                previousCompiledAssemblies.Add(path);
                // Debug.Log("compile assembly "+path);
            };
            CompilationPipeline.compilationFinished += (o)=>{
                var settings = GetScriptAssemblySettings();
                if(settings is null){
                    Debug.LogError("cannot get BeeScriptCompilationState, Unity Version"+Application.unityVersion);
                    return;
                }
                var BuildingForEditor = (bool)settings.GetMemberValue("BuildingForEditor",true);
                previousCompiledRecord.assemblyPathes = previousCompiledAssemblies.ToArray();
                previousCompiledRecord.SaveAsset(); 
                if(!BuildingForEditor){
                    InjectRuntime();
                }else{
                    var assemblies = previousCompiledRecord.GetAssemblies();
                    if(assemblies.Length>0) {
                        InjectEditor(previousCompiledRecord.GetAssemblies());
                    }
                }
            };

        }

        public static void InjectEditor(Assembly[] assemblies) {
            var allInjections = FixHelper.allInjections;
            var freshInjections = FixHelper.GetAllInjections(assemblies);
            var freshAssemblies = freshInjections.Select(inj=>inj.InjectedMethod.DeclaringType.Assembly).Distinct().ToHashSet();
            var outcomeInjections = allInjections.Where(inj=>freshAssemblies.Contains(inj.InjectedMethod.DeclaringType.Assembly)).ToArray();
            Debug.Log($"auto inject {allInjections.Length} injections, {outcomeInjections.Length} to inject");
            if(outcomeInjections.Length>0){
                var isWritten = InjectTargetMode(outcomeInjections,true,activeBuildTarget);

                if(isWritten){
                    Debug.Log($"reload scripts {EditorApplication.isCompiling}");
                    #if UNITY_2019_3_OR_NEWER
                    EditorUtility.RequestScriptReload();
                    #else
                    UnityEditorInternal.InternalEditorUtility.RequestScriptReload();
                    #endif
                }
            }
        }

        public static void InjectRuntime() {
            InjectTargetMode(FixHelper.allInjections,false,activeBuildTarget);
        }

        static bool InjectTargetMode(InjectionAttribute[] injections,bool isEditor, BuildTarget buildTarget) {
            var group = injections.GroupBy(inj=>inj.InjectedMethod.DeclaringType.Assembly);
            var isWritten = false;
            foreach(var g in group){
                var assemblyPath = GetResolvedAssemblyPath(g.Key.GetAssemblyPath(),isEditor,buildTarget);
                isWritten |= VisitAssembly(assemblyPath,g.ToArray(),isEditor,buildTarget);
            }
            return isWritten;
        }

        static string GetResolvedAssemblyPath(string assemblyPath,bool isEditor,BuildTarget buildTarget) {
            var fileName = Path.GetFileName(assemblyPath);
            if (!isEditor){
                var preloads = GetPreloadAssemblies(isEditor, buildTarget);
                var matchedAssembly = preloads.FirstOrDefault(path => Path.GetFileName(path) == fileName);
                if (matchedAssembly is not null)
                {
                    return matchedAssembly;
                }
                var playerAssemblyPath = Path.Join(runtimeOutputDirectory,fileName);
                if(File.Exists(playerAssemblyPath)){
                    return playerAssemblyPath;
                }
            }
            return assemblyPath;
        }

        static string GetEditorAssemblyPath(string assemblyPath)
            => assemblyPath;
        
        static bool VisitAssembly(string assemblyPath,InjectionAttribute[] injections,bool isEditor,BuildTarget buildTarget) {
            var backPath = assemblyPath + BACKUP_EXT;
            var IsEngineAssembly = Path.GetFullPath(assemblyPath)
                .StartsWith(Path.GetFullPath(EditorApplication.applicationContentsPath));

            // engine dlls should be backed up
            if (IsEngineAssembly&&!File.Exists(backPath)) {
                if (!IsFileAvaliable(assemblyPath)) {
                    LogError($"cannot access file: {assemblyPath}");
                    return false;
                }
                File.Copy(assemblyPath, backPath, true);
            }else{
                // File.Copy(backPath, assemblyPath, true);
            }
            // var assemblySearchFolders = GetAssemblySearchFolders(isEditor, buildTarget);
            // var replaceAssemblyPath = miReplace.DeclaringType.Assembly.Location;
            try{
                var inputPath = IsEngineAssembly ? backPath : assemblyPath;
                var isWritten = InjectHelper.InjectAssembly(injections,inputPath,assemblyPath,isEditor,buildTarget);

                if(isWritten) 
                    Debug.Log($"Inject success: {assemblyPath}");
                return isWritten;
            }catch{
                if(IsEngineAssembly){
                    File.Copy(backPath, assemblyPath, true);
                }
                throw;
            }
            
        }

        static bool IsFileAvaliable(string path) {
            try {
                var fs = File.Open(path,FileMode.Open);
                fs?.Close();
                fs?.Dispose();
            } catch {
                return false;
            }
            return true;
        }
        //TODO : Restore
        //TODO : Partial restore on inject 
        // public static void RestoreEditor() {
        //     // var backPath = assemblyPath+".backup";
        //     // var existsBacked = File.Exists(backPath);
        //     RestoreTargetMode(true);
        //     RestoreTargetMode(false);
        //     void RestoreTargetMode(bool isEditor){
        //         var folders = GetAssemblySearchFolders(isEditor,activeBuildTarget);
        //         foreach(var folder in folders){
        //             var backupFiles = Directory.GetFiles(folder,"*"+BACKUP_EXT);
        //         }
        //     }
        // }
        internal static string[] GetAssemblySearchFolders(bool isEditor,BuildTarget buildTarget) {
            return GetPreloadAssemblies(isEditor,buildTarget)
                .Select(Path.GetDirectoryName)
                .Distinct()
                .ToArray();
        }
        internal static string[] GetPreloadAssemblies(bool isEditor,BuildTarget buildTarget) {
            var miGetUnityAssemblies = typeof(InternalEditorUtility).GetMethod("GetUnityAssembliesInternal",bindingFlags);
            var assemblies = (Array)miGetUnityAssemblies.Invoke(null,new object[]{ isEditor, buildTarget, });
            var fiPath = miGetUnityAssemblies.ReturnType.GetElementType().GetField("Path",bindingFlags);
            return assemblies.OfType<object>()
                .Select(a=>fiPath.GetValue(a))
                .OfType<string>()
                .ToArray();
        }
        static object GetScriptAssemblySettings(){
            var t = t_EditorCompilationInterface??=GetType("UnityEditor.CoreModule","EditorCompilationInterface");
            var editorCompilation = t.GetProperty("Instance",bindingFlags).GetValue(null);
            // var compilationType = editorCompilation.GetType();
            if(editorCompilation is null) throw new("cannot get editorCompilation,Unity:"+Application.unityVersion);
            var state = editorCompilation.GetMemberValue("activeBeeBuild")
                ?? editorCompilation.GetMemberValue("_currentBeeScriptCompilationState");
            if(state is null) throw new ("cannot get compile state from editorCompilation,Unity:"+Application.unityVersion);
            return state.GetMemberValue("Settings",true);
        }

        static object GetMemberValue(this object obj,string name,bool IgnoreCase = false){
            var flags = bindingFlags;
            if(IgnoreCase) flags |= BindingFlags.IgnoreCase; 
            var memberInfo = obj.GetType().GetMember(name,flags).FirstOrDefault();
            if(memberInfo is null)
                return null;
                // var fields = obj.GetType().GetFields(flags).Select(f=>"field:"+f.Name);
                // var props = obj.GetType().GetProperties(flags).Select(f=>"prop:"+f.Name);
                // Debug.Log(string.Join("\n",fields)+"\n"+string.Join("\n",props));
                // throw new($"cannot find member {name} in {obj}");
            
            if(memberInfo is FieldInfo fi)
                return fi.GetValue(obj);
            
            if(memberInfo is PropertyInfo pi)
                return pi.GetValue(obj);
            
            if(memberInfo is MethodInfo mi)
                return mi.Invoke(obj,null);
            
            return null;
        }

        static Type GetType(string moduleName, string typeName){
            return System.AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.StartsWith(moduleName+","))
                .SelectMany(a => a.GetTypes())
                .Where(t => t.Name.Equals(typeName))
                .Single();
        }

        static Type t_EditorCompilationInterface;
        // /// <summary>
        // /// scripts recompiled during edtior mode
        // /// </summary>
        // static bool CompiledInEditor = false;
        static string runtimeOutputDirectory;
        static PreviousCompiledRecord previousCompiledRecord=>PreviousCompiledRecord.Instance;
        // static BuildTargetGroup activeBuildTargetGroup => EditorUserBuildSettings.selectedBuildTargetGroup;
        static BuildTarget activeBuildTarget => EditorUserBuildSettings.activeBuildTarget;
        //static StringBuilder strBuilder = new();
        static BindingFlags bindingFlags = 0
            | BindingFlags.Static
            | BindingFlags.Instance
            | BindingFlags.Public
            | BindingFlags.NonPublic
            ;
        static void LogError(string message){
            Debug.LogError(message);
        }
        
    }
}