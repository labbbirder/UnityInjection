using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace com.bbbirder.unity{
    public struct InjectionInfo{
        /// <summary>
        /// indicate the method to be injected
        /// </summary>
        public MethodInfo InjectedMethod;

        //TODO : FixingDelegate?
        // use one of the following to fix target method
        public MethodInfo FixingMethod;
        public Delegate FixingDelegate;


        public Action<Delegate> OriginReceiver;
        // /// <summary>
        // /// the delegate field to store origin method
        // /// </summary>
        // /// <value></value>
        // public FieldInfo OriginSavingField;
        // /// <summary>
        // /// set this if OriginSavingField is not static
        // /// </summary>
        // /// <value></value>
        // public object OriginSavingTarget;

        // public InjectionInfo(MethodInfo injectedMethod,MethodInfo fixingMethod,FieldInfo savingField,object savingTarget){
        //     this.InjectedMethod = injectedMethod;
        //     this.FixingMethod = fixingMethod;
        //     this.FixingDelegate = null;
        //     this.OriginSavingField = savingField;
        //     this.OriginSavingTarget = savingTarget;
        // }
        // public InjectionInfo(MethodInfo injectedMethod,Delegate fixingDelegate,FieldInfo savingField,object savingTarget){
        //     this.InjectedMethod = injectedMethod;
        //     this.FixingMethod = null;
        //     this.FixingDelegate = fixingDelegate;
        //     this.OriginSavingField = savingField;
        //     this.OriginSavingTarget = savingTarget;
        // }
    }
    public abstract class InjectionAttribute:DirectRetrieveAttribute{
        /// <summary>
        /// set this property to populate injections
        /// </summary>
        /// <value></value>
        public abstract IEnumerable<InjectionInfo> ProvideInjections();

        // public MethodInfo InjectedMethod {get;protected set;}
        // public MethodInfo FixingMethod {get;protected set;}
        // public Delegate FixingDelegate {get;protected set;}
        // /// <summary>
        // /// the delegate field to store origin method
        // /// </summary>
        // /// <value></value>
        // public FieldInfo OriginSavingField {get;protected set;}
        // /// <summary>
        // /// set this if OriginSavingField is not static
        // /// </summary>
        // /// <value></value>
        // public object OriginSavingTarget {get;protected set;}
        
    }
}