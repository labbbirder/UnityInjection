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
    }
    public abstract class InjectionAttribute:DirectRetrieveAttribute{
        /// <summary>
        /// set this property to populate injections
        /// </summary>
        /// <value></value>
        public abstract IEnumerable<InjectionInfo> ProvideInjections();
    }
}