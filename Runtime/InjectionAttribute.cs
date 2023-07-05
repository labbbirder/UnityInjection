using System;
using System.Reflection;

namespace com.bbbirder.unity{
    public abstract class InjectionAttribute:DirectRetrieveAttribute{
        public MethodInfo InjectedMethod {get;protected set;}
        public MethodInfo FixingMethod {get;protected set;}
        public Delegate FixingDelegate {get;protected set;}
        /// <summary>
        /// the delegate field to store origin method
        /// </summary>
        /// <value></value>
        public FieldInfo OriginSavingField {get;protected set;}
        /// <summary>
        /// set this if OriginSavingField is not static
        /// </summary>
        /// <value></value>
        public object OriginSavingTarget {get;protected set;}
        
        // public void Deconstruct(out MethodInfo injected,out MethodInfo fixing,out FieldInfo originSaving){
        //     injected = InjectedMethod;
        //     fixing = FixingMethod;
        //     originSaving = OriginSavingField;
        // }
    }
}