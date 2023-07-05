// using UnityEngine;
// using UnityEditor;
// using System.Runtime.CompilerServices;
// using System.IO;

// class TemplateGenerator{
//     [InitializeOnLoadMethod]
//     static void Generate(){
//         var input = File.ReadAllText(GetFilePath());
//         var output = Scriban.Template.Parse(input).Render();
//         File.WriteAllText(GetOutputPath(),output);
//         Debug.Log(output);
//     }
//     static string GetFilePath([CallerFilePath]string path=null)
//         =>Path.Join(path,"../template.txt");
//     static string GetOutputPath([CallerFilePath]string path=null)
//         =>Path.Join(path,"../../Runtime/Decorator-AutoGen.cs");
// }