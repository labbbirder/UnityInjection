using com.bbbirder.unity;
using UnityEngine;
using System;
using UnityEngine.Assertions;

public class Demo:MonoBehaviour{
    void Start(){
        FixHelper.Install();
    }
    void Update(){
        Debug.Log(DemoModal.Salute());
        Debug.Log(new DemoModal().ThisSalute());
        Debug.Log(new DemoModal().Add(4,1.2f));
    }

    #region Fix
    static Func<int> RawSalute;
    [Injection(typeof(DemoModal),nameof(DemoModal.Salute),nameof(RawSalute))]
    static int Salute(){
        return 2;
    }

    static Func<DemoModal,int> RawThisSalute;
    [Injection(typeof(DemoModal),nameof(DemoModal.ThisSalute),nameof(RawThisSalute))]
    static int ThisSalute(DemoModal demo){
        return 2;
    }

    static Func<DemoModal,int,float,float> RawAdd;
    [Injection(typeof(DemoModal),nameof(DemoModal.Add),nameof(RawAdd))]
    static float Add(DemoModal demo,int i,float f){ 
        Debug.Log($"Add {i} + {f} is {i+f}"); 
        return i+f;
    }

    static Action<object> RawLog;
    [Injection(typeof(Debug),nameof(Debug.Log),nameof(RawLog))]
    static void Log(object msg){ 
        RawLog.Invoke("msg:"+msg); 
    }
    #endregion
}