using System;
using System.Reflection;
using UnityEngine.Assertions;
using TargetMethodType = System.Reflection.MethodBase;


namespace com.bbbirder.injection
{
    
    public struct InjectionInfo
    {
        /// <summary>
        /// indicate the method to be injected
        /// </summary>
        public TargetMethodType InjectedMethod { get; private set; }

        //TODO : FixingDelegate?
        // use one of the following to fix target method
        public MethodInfo FixingMethod { get; private set; }
        public Delegate FixingDelegate { get; private set; }


        public Action<Delegate> OriginReceiver;

        public static InjectionInfo Create(TargetMethodType methodToReplace, MethodInfo methodToProvide, Action<Delegate> originMethodReceiver = null)
        {
            Assert.IsNotNull(methodToReplace,"methodToReplace is null");
            Assert.IsNotNull(methodToProvide,"methodToProvide is null");
            return new InjectionInfo()
            {
                InjectedMethod = methodToReplace,
                FixingMethod = methodToProvide,
                OriginReceiver = originMethodReceiver,
            };
        }
        public static InjectionInfo Create(TargetMethodType methodToReplace, Delegate methodToProvide, Action<Delegate> originMethodReceiver = null)
        {
            Assert.IsNotNull(methodToReplace,"methodToReplace is null");
            Assert.IsNotNull(methodToProvide,"methodToProvide is null");
            return new InjectionInfo()
            {
                InjectedMethod = methodToReplace,
                FixingDelegate = methodToProvide,
                OriginReceiver = originMethodReceiver,
            };
        }
        public static InjectionInfo Create<T>(T methodToReplace, T methodToProvide, Action<T> originMethodReceiver = null) where T : Delegate
        {
            Assert.IsNotNull(methodToReplace,"methodToReplace is null");
            Assert.IsNotNull(methodToProvide,"methodToProvide is null");
            return new InjectionInfo()
            {
                InjectedMethod = methodToReplace.Method,
                FixingDelegate = methodToProvide,
                OriginReceiver = f => originMethodReceiver?.Invoke((T)f),
            };
        }
    }
}