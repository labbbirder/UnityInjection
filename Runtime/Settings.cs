using System.Text;

namespace com.bbbirder.unity {
    public static class Settings{
        public const string InjectedMarkNamespace = "com.bbbirder";
        public const string InjectedMarkName = "InjectedMarkAttribute";
        public static string GetDelegateTypeName(string methodName)
            => strBuilder.Clear().Append("__").Append(methodName).Append("Delegate").ToString();

        public static string GetInjectedFieldName(string methodName)
            => strBuilder.Clear().Append("s_").Append(methodName).Append("_injection").ToString();

        public static string GetOriginMethodName(string methodName)
            => strBuilder.Clear().Append("origin_").Append(methodName).ToString();


        static StringBuilder strBuilder = new();
    }
}