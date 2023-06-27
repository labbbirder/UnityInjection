using System;
using System.Runtime.CompilerServices;
using com.bbbirder;

namespace com.bbbirder.unity {
    [AttributeUsage(AttributeTargets.Method)]
    public class InjectionAttribute:DirectRetrieveAttribute{
        public Type type {get;private set;}
        public string methodName {get;private set;}
        public string overwriteName {get;private set;}
        public (string,int) codeLocation;
        public InjectionAttribute(
            Type type,string methodName,string overwriteName,
            [CallerFilePath]string CallerFilePath = null,
            [CallerLineNumber]int CallerLineNumber = 0
        ) {
            this.type = type;
            this.methodName = methodName;
            this.overwriteName = overwriteName;
            this.codeLocation = (CallerFilePath,CallerLineNumber);
        }
        // public InjectionAttribute(Type type){
        //     this.type = type;
        // }
    }
    // [AttributeUsage(AttributeTargets.Assembly,AllowMultiple = true)]
    // public class DirectRetrieveAttribute:Attribute{
    //     public Type type {get;private set;}
    //     public string methodName {get;private set;}
    //     #if NET7_0_OR_GREATER
    //     public int memberIndex {get;private set;}
    //     #endif
    //     public DirectRetrieveAttribute(Type type, string methodName){
    //         this.type = type;
    //         this.methodName = methodName;
    //     }
    //     #if NET7_0_OR_GREATER
    //     public DirectRetrieveAttribute(Type type, int memberIndex){
    //         this.type = type;
    //         this.memberIndex = memberIndex;
    //     }
    //     #endif
    // }
}