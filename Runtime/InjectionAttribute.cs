using System;
using System.Runtime.CompilerServices;

namespace com.bbbirder.unity {
    public class InjectionAttribute:Attribute{
        public Type type {get;private set;}
        public (string,int) codeLocation;
        public string methodName {get;private set;}
        public string overwriteName {get;private set;}
        public InjectionAttribute(
            Type type,string methodName,string overwriteName,
            [CallerFilePath]string CallerFilePath = null,
            [CallerLineNumber]int CallerLineNumber = 0
        ){
            this.type = type;
            this.methodName = methodName;
            this.overwriteName = overwriteName;
            this.codeLocation = (CallerFilePath,CallerLineNumber);
        }
        // public InjectionAttribute(Type type){
        //     this.type = type;
        // }
    }
}