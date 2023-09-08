using UnityEditor;
using UnityEngine;
using System;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;

namespace com.bbbirder.injection.editor
{

    public class InjectionSettingsWindow : EditorWindow
    {
        const string rootUiAssetGUID = "64949e9c31ce0a04b8aa1a3d01d72ab1";
        const string ElementUiAssetGUID = "e0689cb498ad5f8459d2bee8fd022488";
        [MenuItem("Tools/bbbirder/Unity Injection")]
        public static void ShowWindow()
        {
            var window = GetWindow<InjectionSettingsWindow>();
            window.titleContent = new GUIContent("Unity Injection");
            window.Show();
        }
        // public override void SaveChanges()
        // {
        //     base.SaveChanges();
        //     Debug.Log("save");
        // }
        void CreateGUI()
        {
            var settings = InjectionSettings.instance;
            settings.hideFlags &= ~HideFlags.NotEditable;
            var uiAsset = GetVisualTreeAssetByGUID(rootUiAssetGUID);
            var uiEleAsset = GetVisualTreeAssetByGUID(ElementUiAssetGUID);
            uiAsset.CloneTree(rootVisualElement);
            var lst = rootVisualElement.Q<ListView>();

            lst.makeItem = uiEleAsset.CloneTree;
            lst.bindItem = (v, i) =>
            {
                var data = settings.injectionSources[i];
                v.Q<Label>().text = Path.GetFileName(data.path);
            };

            var btnInject = rootVisualElement.Q<Button>("btnInject");
            btnInject.clicked += ()=>{
                UnityInjectUtils.InjectEditor(AppDomain.CurrentDomain.GetAssemblies());
            };

            var serializedObject = new SerializedObject(settings);
            rootVisualElement.Bind(serializedObject);
            rootVisualElement.TrackSerializedObjectValue(serializedObject, so =>
            {
                settings.Save();
            });
        }
        VisualTreeAsset GetVisualTreeAssetByGUID(string guid) =>
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath(guid));
    }
}
