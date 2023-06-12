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
using Assembly = System.Reflection.Assembly;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using InjectionParams = FixHelper.InjectionParams;
public static class UnityInjectUtils{
    const string BACKUP_EXT = ".backup";

    [MenuItem("Tools/bbbirder/Inject for Editor")]
    // [InitializeOnLoadMethod] 
    static void Inject(){
        InjectEditor(AppDomain.CurrentDomain.GetAssemblies());
        // InjectRuntime();
    }
    //TODO: check previous compiled asset & inject editor
    // [InitializeOnLoadMethod] 
    [DidReloadScripts]
    static void TestInject(){
        if(typeof(UnityInjectUtils).Assembly.IsDynamic){
            Debug.LogError("in dynamic module!");
            return;
        }
        Debug.Log("did reload ");
        var assemblies = previousCompiledRecord.GetAssemblies();
        Debug.Log("found compiled pathes: \n"+string.Join("\n",previousCompiledRecord.assemblyPathes));
        if(assemblies.Length>0){
            Debug.Log("found compiled assemblies: \n"+string.Join("\n",assemblies.Select(a=>a.Location)));
            // InjectEditor(assemblies);
            //TODO: Automatically inject for editor
        }
        new VisualElement().FindAncestorUserData();
    }
    [InitializeOnLoadMethod]
    static void Install(){
        var previousCompiledAssemblies = new List<string>();
        CompilationPipeline.compilationStarted += (o)=>{
            Debug.Log("start comp, "+o?.GetMemberValue("Settings",true)?.GetMemberValue("BuildingForEditor",true));
        };
        //TODO: write to previous compiled asset
        CompilationPipeline.assemblyCompilationFinished += (path,msg)=>{
            // var compiledAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(a=>a.Location==path).Single();
            // if(compiledAssembly.MayContainsInjection()) { 
            //     ShouldReload = true; 
            // }
            previousCompiledAssemblies.Add(path);
            Debug.Log(path);
        //     foreach(var m in msg) Debug.Log(m);
        };
        CompilationPipeline.compilationFinished += (o)=>{
            var BuildingForEditor = (bool)o?.GetMemberValue("Settings",true)?.GetMemberValue("BuildingForEditor",true);
            previousCompiledRecord.assemblyPathes = previousCompiledAssemblies.ToArray();
            previousCompiledRecord.SaveAsset(); 
            if(!BuildingForEditor){
                //TODO: Inject Runtime
                InjectRuntime();
            // }else{
                // CompiledInEditor = true;
            }
            Debug.Log("finissh comp");
        };
        Debug.Log("Initial on load");
    }

    public static void InjectEditor(Assembly[] assemblies){
        var injections = FixHelper.GetAllInjections(assemblies);

        if(injections.Length>0){
            InjectTargetMode(injections,true,activeBuildTarget);
            #if UNITY_2019_3_OR_NEWER
            Debug.Log("RequestScriptReload");
            EditorUtility.RequestScriptReload();
            #endif
        }
    }

    public static void InjectRuntime(){
        var injections = FixHelper.GetAllInjections();
        InjectTargetMode(injections,false,activeBuildTarget);
    }

    static void InjectTargetMode(InjectionParams[] injections,bool isEditor, BuildTarget buildTarget){
        var group = injections.GroupBy(inj=>inj.targetType.Assembly);
        foreach(var g in group){
            var assemblyPath = GetResolvedAssemblyPath(g.Key.GetAssemblyPath(),isEditor,buildTarget);
            VisitAssembly(assemblyPath,g.ToArray(),isEditor,buildTarget);
        }
    }
    static string GetResolvedAssemblyPath(string assemblyPath,bool isEditor,BuildTarget buildTarget){
        if (!isEditor){
            var preloads = GetPreloadAssemblies(isEditor, buildTarget);
            var matchedAssembly = preloads.FirstOrDefault(path => Path.GetFileName(path) == Path.GetFileName(assemblyPath));
            if (matchedAssembly is not null)
            {
                assemblyPath = matchedAssembly;
            }
        }
        return assemblyPath;
    }

    static string GetEditorAssemblyPath(string assemblyPath)
        => assemblyPath;
    
    static void VisitAssembly(string assemblyPath,InjectionParams[] injections,bool isEditor,BuildTarget buildTarget) {
        var backPath = assemblyPath + BACKUP_EXT;
        var existsBacked = File.Exists(backPath);

        if (!existsBacked) {
           if (!IsFileAvaliable(assemblyPath)) {
               LogError($"cannot access file: {assemblyPath}");
               return;
           }
           File.Copy(assemblyPath, backPath, true);
        }
        // var assemblySearchFolders = GetAssemblySearchFolders(isEditor, buildTarget);
        // var replaceAssemblyPath = miReplace.DeclaringType.Assembly.Location;

        InjectHelper.InjectAssembly(injections,backPath,assemblyPath);
        Debug.Log($"Inject success: {assemblyPath}");
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
    public static void RestoreEditor() {
        // var backPath = assemblyPath+".backup";
        // var existsBacked = File.Exists(backPath);
        RestoreTargetMode(true);
        RestoreTargetMode(false);
        void RestoreTargetMode(bool isEditor){
            var folders = GetAssemblySearchFolders(isEditor,activeBuildTarget);
            foreach(var folder in folders){
                var backupFiles = Directory.GetFiles(folder,"*"+BACKUP_EXT);
            }
        }
    }
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

    // public static bool IsSameName(this TypeReference left, TypeReference right)
    // {
    //     return left.FullName == right.FullName;
    // }
    public static bool IsApproximatelySameWith(this MethodDefinition left, MethodDefinition right)
    {
        // Debug.Log($"{left.Parameters.Count != right.Parameters.Count},{!left.ReturnType.IsSameName(right.ReturnType)},{left.HasThis != right.HasThis}");
        // if (left.Parameters.Count != right.Parameters.Count
        //             // || left.Name != right.Name
        //             // || !left.ReturnType.IsSameName(right.ReturnType)
        //             // || !left.DeclaringType.IsSameName(right.DeclaringType)
        //             // || left.HasThis != right.HasThis
        //             // || left.GenericParameters.Count != right.GenericParameters.Count
        // )
        // {
        //     return false;
        // }

        // for (int i = 0; i < left.Parameters.Count; i++)
        // {
        //     if (left.Parameters[i].Attributes != right.Parameters[i].Attributes
        //         || !left.Parameters[i].ParameterType.IsSameName(right.Parameters[i].ParameterType))
        //     {
        //         return false;
        //     }
        // }

        // for (int i = 0; i < left.GenericParameters.Count; i++)
        // {
        //     if (left.GenericParameters[i].IsSameName(right.GenericParameters[i]))
        //     {
        //         return false;
        //     }
        // } 

        return true;
    }
    static object GetMemberValue(this object obj,string name,bool IgnoreCase = false){
        var flags = bindingFlags;
        if(IgnoreCase) flags |= BindingFlags.IgnoreCase; 
        var memberInfo = obj.GetType().GetMember(name,flags).FirstOrDefault();
        if(memberInfo is null){
            throw new($"cannot find member {name} in {obj}");
        }
        if(memberInfo is FieldInfo fi){
            return fi.GetValue(obj);
        }
        if(memberInfo is PropertyInfo pi){
            return pi.GetValue(obj);
        }
        if(memberInfo is MethodInfo mi){
            return mi.Invoke(obj,null);
        }
        return null;
    }
    // /// <summary>
    // /// scripts recompiled during edtior mode
    // /// </summary>
    // static bool CompiledInEditor = false;
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