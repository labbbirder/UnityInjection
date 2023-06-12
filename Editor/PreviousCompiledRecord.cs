
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class PreviousCompiledRecord:ScriptableObject{
    const string savePath = "Temp/PreviousCompiledRecord.asset";
    [SerializeField]
    public string[] assemblyPathes = new string[]{};
    static PreviousCompiledRecord m_Instance;
    public static PreviousCompiledRecord Instance {
        get{
            if(!m_Instance){
                m_Instance = (PreviousCompiledRecord)InternalEditorUtility.LoadSerializedFileAndForget(savePath).FirstOrDefault();
            }
            if(!m_Instance){
                m_Instance = CreateInstance<PreviousCompiledRecord>();
            }
            return m_Instance;
        }
    }
    public Assembly[] GetAssemblies(){
        var hashset = assemblyPathes
            .Select(p=>Path.Join(Directory.GetCurrentDirectory(),p))
            .Select(p=>p.Replace('\\','/'))
            .ToHashSet();
        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(a=>hashset.Contains(a.GetAssemblyPath()))
            .ToArray();
    }
    public void SaveAsset(string path=savePath){
        InternalEditorUtility.SaveToSerializedFileAndForget(new[]{this},path,true);
    }
}