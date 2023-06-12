using System.Text;

public class Settings{

    public static string GetDelegateTypeName(string methodName)
        => strBuilder.Clear().Append("__").Append(methodName).Append("Delegate").ToString();

    public static string GetInjectedFieldName(string methodName)
        => strBuilder.Clear().Append("s_").Append(methodName).Append("_injection").ToString();

    public static string GetOriginMethodName(string methodName)
        => strBuilder.Clear().Append("origin_").Append(methodName).ToString();


    static StringBuilder strBuilder = new();
}