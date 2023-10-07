
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;

namespace com.bbbirder.injection.editor
{
    [FilePath(savePath, FilePathAttribute.Location.ProjectFolder)]
    public class InjectionSettings : ScriptableSingleton<InjectionSettings>
    {
        const string savePath = "Library/InjectionSettings.asset";
        [Serializable]
        public struct AssemblyRecord
        {
            public string path;
            public long lastModifyTime;
        }


        [SerializeField]
        public bool enabled = true;

        [SerializeField]
        bool autoInjectEditor = true;

        [SerializeField]
        bool autoInjectBuild = true;

        [SerializeField]
        public List<AssemblyRecord> injectionSources = new();
        
        [SerializeField]
        public List<string> compilationErrorAssemblies = new();

        public bool ShouldAutoInjectEditor => enabled && autoInjectEditor;
        public bool ShouldAutoInjectBuild => enabled && autoInjectBuild;

        public void Save(){
            base.Save(true);
        }

        // public Assembly[] GetAssemblies()
        // {
        //     var hashset = injectionSources
        //         .Select(r => Path.Join(Directory.GetCurrentDirectory(), r.path))
        //         .Select(p => p.Replace('\\', '/'))
        //         .ToHashSet();
        //     return AppDomain.CurrentDomain.GetAssemblies()
        //         .Where(a => hashset.Contains(a.GetAssemblyPath()))
        //         .ToArray();
        // }

        public void SetInjectionSources(IEnumerable<string> sources)
        {
            injectionSources.Clear();
            foreach (var path in sources)
            {
                var lwts = File.GetLastWriteTimeUtc(path).ToFileTimeUtc();
                injectionSources.Add(new()
                {
                    path = path,
                    lastModifyTime = lwts
                });
            }
            Save();
        }

        public void GetOutdatedSources(IEnumerable<string> sources, List<string> outdated)
        {
            var srcDict = injectionSources.ToDictionary(r => r.path, r => r.lastModifyTime);
            outdated.AddRange(
                sources.Where(s =>
                    !srcDict.TryGetValue(s, out var lwts) || lwts < File.GetLastWriteTimeUtc(s).ToFileTimeUtc()
                )
            );
        }
        public bool CheckShouldUpdate(string path)
        {
            var srcDict = injectionSources.ToDictionary(r => r.path, r => r.lastModifyTime);
            if (!srcDict.TryGetValue(path, out var lwts)) return true;
            if (lwts < File.GetLastWriteTimeUtc(path).ToFileTimeUtc()) return true;
            return false;
        }
    }
}