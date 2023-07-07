using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using com.bbbirder.unity;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditorInternal;
using UnityEditor;
using UnityEditor.Compilation;

namespace com.bbbirder.unityeditor {
    public static class InjectHelper{

        /// <summary>
        /// inject target assembly
        /// </summary>
        /// <param name="injections"></param>
        /// <param name="inputAssemblyPath"></param>
        /// <param name="outputAssemblyPath"></param>
        /// <returns>is written</returns>
        internal static bool InjectAssembly(InjectionAttribute[] injections, string inputAssemblyPath,string outputAssemblyPath,bool isEditor,BuildTarget buildTarget) {
            // set up assembly resolver
            var resolver = new DefaultAssemblyResolver();
            var apiCompatibilityLevel = PlayerSettings.GetApiCompatibilityLevel(EditorUserBuildSettings.selectedBuildTargetGroup);
            var assemblySearchFolders = UnityInjectUtils.GetAssemblySearchFolders(isEditor, buildTarget);
            var systemAssemblyDirectories = CompilationPipeline.GetSystemAssemblyDirectories(apiCompatibilityLevel);
            resolver.AddSearchDirectory(Path.GetDirectoryName(outputAssemblyPath));
            foreach(var folder in assemblySearchFolders){
               resolver.AddSearchDirectory(folder);
            }
            foreach(var folder in systemAssemblyDirectories){
               resolver.AddSearchDirectory(folder);
            }

            var IsPlayerAssembly = Path.GetFullPath(inputAssemblyPath).StartsWith(Path.GetFullPath("Library/"))
                                || Path.GetFullPath(inputAssemblyPath).StartsWith(Path.GetFullPath("Temp/"));

            var targetAssembly = AssemblyDefinition.ReadAssembly(inputAssemblyPath, new ReaderParameters(){
                AssemblyResolver=resolver,
                ReadingMode=ReadingMode.Immediate,
                ReadSymbols = IsPlayerAssembly,
                InMemory = true,
            });

            //mark check
            var injected = targetAssembly.MainModule.Types.Any(t=>
                Settings.InjectedMarkName == t.Name &&
                Settings.InjectedMarkNamespace ==t.Namespace);
            if(injected){
                targetAssembly.Release();
                return false;
            }

            foreach(var injection in injections){
                var type = injection.InjectedMethod.DeclaringType;
                var methodName = injection.InjectedMethod.Name;
                var targetType = targetAssembly.MainModule.Types
                    .Where(t => IsSameType(type,t))
                    .SingleOrDefault();
                if(targetType is null){
                    throw new($"Cannot find Type `{type}` in target assembly {inputAssemblyPath}");
                }
                var targetMethod = targetType.FindMethod(methodName).Resolve();
                if(targetMethod is null){
                    throw new($"Cannot find Method `{methodName}` in Type `{type}`");
                }

                //add origin
                var originalMethod = targetType.DuplicateOriginalMethod(targetMethod);
                //add field
                var (field,fieldInvoke) = targetType.AddInjectField(targetMethod,methodName);
                //add method
                targetType.AddInjectionMethod(targetMethod,originalMethod,field,fieldInvoke,methodName);
            }

            //mark make
            var InjectedMark = new TypeDefinition(
                Settings.InjectedMarkNamespace,
                Settings.InjectedMarkName,
                TypeAttributes.Class,
                targetAssembly.MainModule.TypeSystem.Object);
            targetAssembly.MainModule.Types.Add(InjectedMark);


            targetAssembly.Write(outputAssemblyPath,new WriterParameters(){
                WriteSymbols = IsPlayerAssembly,
            });
            targetAssembly.Release();
            return true;

            static bool IsSameType(Type t1,TypeDefinition t2){
                var isSameNamespace = t1.Namespace==t2.Namespace;
                if(string.IsNullOrEmpty(t1.Namespace) && string.IsNullOrEmpty(t2.Namespace)){
                    isSameNamespace = true;
                }
                return t1.Name==t2.Name && isSameNamespace;
            }
        }
        static MethodDefinition DuplicateOriginalMethod(this TypeDefinition targetType,MethodDefinition targetMethod){
            var originName = Settings.GetOriginMethodName(targetMethod.Name);
            var duplicatedMethod = targetMethod.Clone();
            duplicatedMethod.IsPrivate = true;
            duplicatedMethod.Name = originName;
            targetType.Methods.Add(duplicatedMethod);
            return duplicatedMethod;
        }
        static void Release(this AssemblyDefinition assemblyDefinition){
            if(assemblyDefinition == null) return;
            assemblyDefinition.MainModule.AssemblyResolver?.Dispose();
            assemblyDefinition.MainModule.SymbolReader?.Dispose();
            assemblyDefinition.Dispose();
        }
        static (FieldDefinition,MethodReference) AddInjectField(this TypeDefinition targetType,MethodDefinition targetMethod,string methodName){
            var injectionName = Settings.GetInjectedFieldName(methodName);
            var HasThis = targetMethod.HasThis;
            var Parameters = targetMethod.Parameters;
            var GenericParameters = targetMethod.GenericParameters;
            var CustomAttributes = targetMethod.CustomAttributes;
            var ReturnType = targetMethod.ReturnType;
            var ReturnVoid = targetMethod.IsReturnVoid();
            //define delegate
            // var delegateParameters = new List<TypeReference>();
            // if(HasThis) delegateParameters.Add(targetType);
            // foreach(var p in Parameters) delegateParameters.Add(p.ParameterType);
            // var delegateType = targetType.Module.CreateDelegateType(Settings.GetDelegateTypeName(methodName),targetType,ReturnType,delegateParameters);
            // targetType.NestedTypes.Add(delegateType);
            
            var genName = targetMethod.IsReturnVoid()?"System.Action`":"System.Func`";
            var genPCnt = Parameters.Count;
            if(!ReturnVoid) genPCnt++;
            if(HasThis) genPCnt++;
            var rawGenType = targetType.Module.FindType(Type.GetType(genName+genPCnt));
            var genType = targetType.Module.ImportReference(rawGenType);
            var genInst = new GenericInstanceType(genType);
            if(HasThis)
                genInst.GenericArguments.Add(targetType);
            foreach(var p in Parameters)
                genInst.GenericArguments.Add(p.ParameterType);
            if(!ReturnVoid)
                genInst.GenericArguments.Add(ReturnType);
            //store fields
            var sfldInject = new FieldDefinition(injectionName,
                FieldAttributes.Private|FieldAttributes.Static,
                genInst);
            // var sfldOrigin = new FieldDefinition(originName,
            //     FieldAttributes.Private|FieldAttributes.Static|FieldAttributes.Assembly,
            //     targetType.Module.ImportReference(typeof(Delegate)));
            // var resMth = genInst.Resolve();
            var genMtd = rawGenType.FindMethod("Invoke");
            // genMtd.DeclaringType = genInst;
            var mnlMth = new MethodReference(genMtd.Name,genMtd.ReturnType,genInst){
                ExplicitThis = false,
                HasThis = true,
                CallingConvention = genMtd.CallingConvention
            };
            foreach(var p in genMtd.Parameters)
                mnlMth.Parameters.Add(p);

            targetType.Fields.Add(sfldInject);
            return (sfldInject,mnlMth);
        }

        static void AddInjectionMethod(
            this TypeDefinition targetType,
            MethodDefinition targetMethod,MethodDefinition originalMethod,
            FieldDefinition delegateField,MethodReference fieldInvoke,string methodName
        ){
            var argidx = 0;
            var HasThis =           targetMethod.HasThis;
            var Parameters =        targetMethod.Parameters;
            // var GenericParameters = targetMethod.GenericParameters;
            // var CustomAttributes =  targetMethod.CustomAttributes;
            var ReturnType =        targetMethod.ReturnType;

            //redirect method
            targetMethod.Body.Instructions.Clear();
            var delegateType = delegateField.FieldType.Resolve();
            var ilProcessor = targetMethod.Body.GetILProcessor();
            var tagOp = Instruction.Create(OpCodes.Nop);
            //check null
            ilProcessor.Append(Instruction.Create(OpCodes.Ldsfld,  delegateField));
            ilProcessor.Append(Instruction.Create(OpCodes.Brtrue_S,tagOp));
            
            // //set field
            // if(HasThis)
            //     ilProcessor.Append(Instruction.Create(OpCodes.Ldarg_0));
            // else
            //     ilProcessor.Append(Instruction.Create(OpCodes.Ldnull));
            // ilProcessor.Append(Instruction.Create(OpCodes.Ldftn, originalMethod));
            // ilProcessor.Append(Instruction.Create(OpCodes.Newobj,delegateType.FindMethod(".ctor")));
            // ilProcessor.Append(Instruction.Create(OpCodes.Stsfld,delegateField));

            //invoke origin
            argidx = 0;
            if(HasThis)
                ilProcessor.Append(ilProcessor.createLdarg(argidx++));
            for(var i=0; i<Parameters.Count; i++){
                var pType = Parameters[i].ParameterType;
                ilProcessor.Append(ilProcessor.createLdarg(argidx++));
                // if(pType.IsValueType)
                //     ilProcessor.Append(Instruction.Create(OpCodes.Box,pType));
            }
            if(HasThis)
                ilProcessor.Append(Instruction.Create(OpCodes.Callvirt,originalMethod));
            else
                ilProcessor.Append(Instruction.Create(OpCodes.Call,originalMethod));
            // if(originalMethod.IsReturnVoid())
            //     ilProcessor.Append(Instruction.Create(OpCodes.Pop));
            ilProcessor.Append(Instruction.Create(OpCodes.Ret));

            //invoke
            ilProcessor.Append(tagOp);
            ilProcessor.Append(Instruction.Create(OpCodes.Ldsfld,delegateField));
            argidx = 0;
            if(HasThis)
                ilProcessor.Append(ilProcessor.createLdarg(argidx++));
            for(var i=0; i<Parameters.Count; i++){
                var pType = Parameters[i].ParameterType;
                ilProcessor.Append(ilProcessor.createLdarg(argidx++));
                // if(pType.IsValueType)
                //     ilProcessor.Append(Instruction.Create(OpCodes.Box,pType));
            }
            ilProcessor.Append(Instruction.Create(OpCodes.Callvirt,
                (fieldInvoke)));
            
            // Fixes: conditional boxing here is unnecessary
            // if(ReturnType.IsComplexValueType())
            //     ilProcessor.Append(Instruction.Create(OpCodes.Box,ReturnType));
            
            ilProcessor.Append(Instruction.Create(OpCodes.Nop));
            ilProcessor.Append(Instruction.Create(OpCodes.Ret));
        }
        // static void InjectCctor(this TypeDefinition targetType,FieldDefinition field){
        //     var cctorMethod = targetType.Methods.FirstOrDefault(m=>m.Name==".cctor");
        //     if(cctorMethod is null){
        //         cctorMethod = new MethodDefinition(".cctor",MethodAttributes.Static|MethodAttributes.Private,targetType.Module.TypeSystem.Void);
        //         targetType.Methods.Add(cctorMethod);
        //     }
        //     var ilProcessor = cctorMethod.Body.GetILProcessor();
        //     var bdis = cctorMethod.Body.Instructions;
        //     var insertPoint = bdis[0];
        //     ilProcessor.InsertBefore(insertPoint,Instruction.Create(OpCodes.Ldsfld,field));
        //     ilProcessor.InsertBefore(insertPoint,Instruction.Create(OpCodes.Ldsfld,field));
        // }
        // =>md.GetType(type.ToString(),true);
        static Instruction createLdarg(this ILProcessor ilProcessor, int i)
        {
            if (i < s_ldargs.Length)
            {
                return Instruction.Create(s_ldargs[i]);
            }
            else if (i < 256)
            {
                return ilProcessor.Create(OpCodes.Ldarg_S, (byte)i);
            }
            else
            {
                return ilProcessor.Create(OpCodes.Ldarg, (short)i);
            }
        }

        /// <summary>
        /// Create a clone of the given method definition
        /// </summary>
        public static MethodDefinition Clone(this MethodDefinition source)
        {
            var result = new MethodDefinition(source.Name, source.Attributes, source.ReturnType) {
                ImplAttributes = source.ImplAttributes,
                SemanticsAttributes = source.SemanticsAttributes,
                HasThis = source.HasThis,
                ExplicitThis = source.ExplicitThis,
                CallingConvention = source.CallingConvention
            };
            foreach (var p in source.Parameters)       result.Parameters.Add(p);
            foreach (var p in source.CustomAttributes) result.CustomAttributes.Add(p);
            foreach (var p in source.GenericParameters)result.GenericParameters.Add(p);
            if (source.HasBody)
            {
                result.Body = source.Body.Clone(result);
            }
            return result;
        }

        /// <summary>
        /// Create a clone of the given method body
        /// </summary>
        public static MethodBody Clone(this MethodBody source, MethodDefinition target)
        {
            var result = new MethodBody(target) { InitLocals = source.InitLocals, MaxStackSize = source.MaxStackSize };
            var worker = result.GetILProcessor();
            if(source.HasVariables){
                foreach(var v in source.Variables){
                    result.Variables.Add(v);
                }
            }
            foreach (var i in source.Instructions)
            {
                // Poor mans clone, but sufficient for our needs
                var clone = Instruction.Create(OpCodes.Nop);
                clone.OpCode = i.OpCode;
                clone.Operand = i.Operand;
                worker.Append(clone);
            }
            return result;
        }

        internal static bool IsReturnVoid(this MethodDefinition md)
            => md.ReturnType.ToString()==voidType.ToString();
        internal static bool IsReturnValueType(this MethodDefinition md)
            => !md.IsReturnVoid() && md.ReturnType.IsValueType;
        internal static bool IsComplexValueType(this TypeReference td)
            => td.ToString()!=voidType.ToString() && !td.IsPrimitive;
        internal static Type GetUnderlyingType(this TypeReference td)
            => td.IsPrimitive ? Type.GetType(td.Name) : objType;
        internal static MethodReference FindMethod(this TypeDefinition td,string methodName)
            => td.Module.ImportReference(td.Methods.FirstOrDefault(m=>m.Name==methodName));
        internal static TypeDefinition FindType(this ModuleDefinition md,Type type){
            HashSet<string> knownAssemblyNames = new();
            List<ModuleDefinition> modules = new();
            GetModules(md);
            foreach(var m in modules){
                var tp = m.GetType(type.Namespace,type.Name);
                if(null != tp){
                    return tp;
                }
            }
            return null;
            void GetModules(ModuleDefinition md){
                if(knownAssemblyNames.Contains(md.FileName))
                    return;
                var refModules = md.AssemblyReferences
                    .Select(an=>{
                        try{
                            return md.AssemblyResolver.Resolve(an).MainModule;
                        }catch{
                            return null;
                        }
                    })
                    .Where(r=>r != null)
                    .ToArray();
                AddModule(md);
                foreach(var m in refModules){
                    GetModules(m);
                }
            }
            void AddModule(ModuleDefinition md){
                var fileName = md.FileName;
                if(!knownAssemblyNames.Contains(fileName))
                    modules.Add(md);
                knownAssemblyNames.Add(fileName);
            }
            // return new TypeReference(type.Namespace,type.Name,md,md.TypeSystem.CoreLibrary);
        }
        internal static TypeReference FindType<T>(this ModuleDefinition md){
            return FindType(md,typeof(T));
            // return new TypeReference(typeof(T).Namespace,typeof(T).Name,md,md.TypeSystem.CoreLibrary);
        }
        internal static TypeDefinition CreateDelegateType(this ModuleDefinition assembly,string name,TypeDefinition declaringType,
                TypeReference returnType, IEnumerable<TypeReference> parameters)
        {
            var voidType = assembly.TypeSystem.Void;
            var objectType = assembly.TypeSystem.Object;
            var nativeIntType = assembly.TypeSystem.IntPtr;
            var asyncResultType = assembly.FindType<IAsyncResult>();
            var asyncCallbackType = assembly.FindType<AsyncCallback>();
            var multicastDelegateType = assembly.FindType<MulticastDelegate>();

            var DelegateTypeAttributes = TypeAttributes.NestedPublic | TypeAttributes.Sealed;
            var dt = new TypeDefinition("", name, DelegateTypeAttributes, multicastDelegateType);
            dt.DeclaringType = declaringType;

            // add constructor
            var ConstructorAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var constructor = new MethodDefinition(".ctor", ConstructorAttributes, voidType);
            constructor.Parameters.Add(new ParameterDefinition("objectInstance", ParameterAttributes.None, objectType));
            constructor.Parameters.Add(new ParameterDefinition("functionPtr", ParameterAttributes.None, nativeIntType));
            constructor.ImplAttributes = MethodImplAttributes.Runtime;
            dt.Methods.Add(constructor);

            // add BeginInvoke
            var DelegateMethodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.VtableLayoutMask;
            var beginInvoke = new MethodDefinition("BeginInvoke", DelegateMethodAttributes, asyncResultType);
            foreach (var p in parameters)
            {
                beginInvoke.Parameters.Add(new ParameterDefinition(p));
            }
            beginInvoke.Parameters.Add(new ParameterDefinition("callback", ParameterAttributes.None, asyncCallbackType));
            beginInvoke.Parameters.Add(new ParameterDefinition("object", ParameterAttributes.None, objectType));
            beginInvoke.ImplAttributes = MethodImplAttributes.Runtime;
            dt.Methods.Add(beginInvoke);

            // add EndInvoke
            var endInvoke = new MethodDefinition("EndInvoke", DelegateMethodAttributes, returnType);
            endInvoke.Parameters.Add(new ParameterDefinition("result", ParameterAttributes.None, asyncResultType));
            endInvoke.ImplAttributes = MethodImplAttributes.Runtime;
            dt.Methods.Add(endInvoke);

            // add Invoke
            var invoke = new MethodDefinition("Invoke", DelegateMethodAttributes, returnType);
            foreach (var p in parameters)
            {
                // if(!p.IsValueType){
                //     invoke.Parameters.Add(new ParameterDefinition(p.Name,ParameterAttributes.In,objectType));
                // }else{
                    invoke.Parameters.Add(new ParameterDefinition(p));
                // }
            }
            invoke.ImplAttributes = MethodImplAttributes.Runtime;
            dt.Methods.Add(invoke);

            return dt;
        }
        static Type voidType = typeof(void);
        static Type objType = typeof(object);
        static OpCode[] s_ldargs = new []{OpCodes.Ldarg_0,OpCodes.Ldarg_1,OpCodes.Ldarg_2,OpCodes.Ldarg_3};
    }
}