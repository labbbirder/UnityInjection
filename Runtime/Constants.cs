using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace com.bbbirder.injection
{
    public static class Constants
    {
        public const string INJECTED_MARK_NAMESPACE = "com.bbbirder";
        public const string INJECTED_MARK_NAME = "InjectedMarkAttribute";

        public static string GetInjectedFieldName(string methodName, string methodSignature)
            => strBuilder.Clear().Append("_injection_field+").Append(methodName).Append(MD5Hash(methodSignature)).ToString();

        public static string GetOriginMethodName(string methodName, string methodSignature)
            => strBuilder.Clear().Append("_injection_origin+").Append(methodName).Append(MD5Hash(methodSignature)).ToString();

        static string MD5Hash(string rawContent)
        {
            var md5 = MD5.Create();
            var buffer = md5.ComputeHash(Encoding.UTF8.GetBytes(rawContent));
            return string.Concat(buffer.Select(b => b.ToString("X")));
        }

        static StringBuilder strBuilder = new();
    }
}