using com.bbbirder.injection;
using UnityEngine;
using System;
using UnityEngine.Assertions;
using System.Reflection;
using System.Collections.Generic;

public class Demo : MonoBehaviour
{
    void Start()
    {
        // FixHelper.InstallAll();
        print("press SPACE to replace methods");
    }
    
    void Update()
    {
        Debug.Log(DemoModal.Salute());
        Debug.Log(new DemoModal().ThisSalute());
        Debug.Log(new DemoModal().Add(4, 1.2f));
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FixHelper.InstallAll();
        }
    }

    #region Fix
    static Func<int> RawSalute;
    static int Salute()
    {
        return 2;
    }

    static Func<DemoModal, int> RawThisSalute;
    static int ThisSalute(DemoModal demo)
    {
        return 2;
    }

    static Func<DemoModal, int, float, float> RawAdd;
    static float Add(DemoModal demo, int i, float f)
    {
        Debug.Log($"Add {i} + {f} is {i + f}");
        return i + f;
    }

    static Action<object> RawLog;
    [HideInCallstack]
    static void Log(object msg)
    {
        RawLog?.Invoke("msg:" + msg);
    }

    internal class MethodReplacer : IInjection
    {
        static BindingFlags bindingFlags = 0
            | BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Static
            ;
        public IEnumerable<InjectionInfo> ProvideInjections()
        {
            yield return InjectionInfo.Create<Func<int>>(
                DemoModal.Salute,
                Salute,
                f => RawSalute = f
            );
            yield return InjectionInfo.Create(
                typeof(DemoModal).GetMethod(nameof(DemoModal.ThisSalute)),
                typeof(Demo).GetMethod(nameof(Demo.ThisSalute), bindingFlags),
                f => RawThisSalute = (Func<DemoModal, int>)f
            );
            yield return InjectionInfo.Create(
                typeof(DemoModal).GetMethod(nameof(DemoModal.Add)),
                typeof(Demo).GetMethod(nameof(Demo.Add), bindingFlags),
                f => RawAdd = (Func<DemoModal, int, float, float>)f
            );
            yield return InjectionInfo.Create<Action<object>>(
                Debug.Log,
                Log,
                f => RawLog = f
            );
        }

    }
    #endregion

}