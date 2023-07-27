# UnityInjection
Unity注入模块，可以运行时改变被注入函数实现。

![mono support](http://img.shields.io/badge/Mono-support-green)
![il2cpp support](http://img.shields.io/badge/IL2CPP-support-green)
![GitHub last commit](http://img.shields.io/github/last-commit/labbbirder/UnityInjection)
![GitHub package.json version](http://img.shields.io/github/package-json/v/labbbirder/UnityInjection)

Verified: unity 2021, unity 2022

## Purpose
开发此模块的最初动机是修改UnityEngine的源码。对这样一个具体问题一般化分析和设计后，最终实现顺便支持了用户自定义的装饰器和AOP。

### 是个轮子？
Unity的注入模块已经有一些其他大神的实现了，为什么还要造一个轮子呢？原因如下：
1. 基于UnityEditor。一些注入模块，需要依赖外部工具；而此实现在完全在UnityEditor下注入。
2. 支持IL2CPP。一些注入模块只支持Editor和Mono；而此实现支持所有平台和选项。
3. 支持修改引擎源码。一些注入模块只能修改用户代码，无法注入引擎代码；而此实现可以注入引擎和用户代码。


### 直接修改DLL文件？
直接修改DLL文件是可行的，但是存在以下问题：
1. 无法记录所做的修改
2. 不方便团队共享和版本控制
3. 不能改变Unity版本
4. 修改步骤繁琐，容易误操作

## Quick Start
### Installation
**via git url**

step 1. 安装依赖库：[DirectRetrieveAttribute](https://github.com/labbbirder/DirectRetrieveAttribute#安装)

step 2. 通过git url安装

**via openupm** (recommend)

execute command line：

```bash
openupm add com.bbbirder.injection
```
### Basic Usage
一个修改`Debug.Log`的例子
```csharp
using com.bbbirder.unity;
using UnityEngine;

// this illustration shows how to hook `Debug.Log`
public class FirstPatch
{
    // the field to be overwrited to original method
    static Action<object> RawLog;

    [Fixer(typeof(Debug),"Log",nameof(RawLog))]
    static void Log(object msg){
        return RawLog.Invoke("[msg] "+msg); 
    }
}

```
`FixerAttribute`接收3个参数：
  1. 目标类型
  2. 目标方法名称
  3. 用于保存原方法的函数成员名称

初始化的时候调用：
```csharp
FixHelper.Install();// 查找所有注入标记，并使生效
```
测试成果：
```csharp
Debug.Log("hello"); //output: [msg] hello
```
### 装饰器
值得一提的是，装饰器的实现有以下优势：
* 调用是0GC，低开销的
* 实现只需要指定一个`DecoratorAttribute`
* 支持装饰异步函数

下面的例子实现目标方法执行前和后打印信息

定义一个装饰器，需要继承自`DecoratorAttribute`，并实现`Decorate`方法
```csharp
public class DebugInvocationAttribute:DecoratorAttribute
{
    string info;

    public DebugInvocationAttribute(string _info)
    {
        info = _info;
    }

    void OnCompleted()
    {
        Debug.Log("end "+info);
    }

    protected override R Decorate<R>(InvocationInfo<R> invocation)
    {
        Debug.Log("begin "+info+string.Join(",",invocation.Arguments));
        // invoke original method
        var r = invocation.FastInvoke();
        if(IsAsyncMethod)
        {
            // delay on async method
            invocation.GetAwaiter(r).OnCompleted(OnCompleted);
        }
        else
        {
            OnCompleted();
        }
        return r;
    }
}

```

使用

```csharp

public class Demo:MonoBehaviour{
    
    void Start(){
        FixHelper.Install();
    }

    async void Update(){
        if(Input.GetKeyDown(KeyCode.A)){
            Work(1,"foo");
        }
        if(Input.GetKeyDown(KeyCode.S)){
            await AsyncWork(2,"bar");
            print("return");
        }
    }

    //decorate a standard method
    [DebugInvocation("w1")]
    int Work(int i, string s){
        Debug.Log("do work");
        return 123+i;
    }

    //decorate an async method
    [DebugInvocation("aw2")]
    async Task<int> AsyncWork(int i, string s){
        Debug.Log("do a lot work");
        await Task.Delay(1000);
        return 123+i;
    }
}
```
### 异步方法补充说明
async方法有一个值得斟酌的问题。如果上例的Task换成UniTask。则不会打印return，这是因为Decrote方法覆盖了原本的continuationAction（这与UniTask的实现有关）。使用如下Decorate方法可以解决所有此类问题：
```csharp
    protected override R Decorate<R>(InvocationInfo<R> invocation)
    {
        Debug.Log("begin "+info+string.Join(",",invocation.Arguments));
        var r = invocation.FastInvoke();
        if(IsAsyncMethod)
        {
            // delay when its an async method
            var awaiter = invocation.GetAwaiter(r);
            UniTask.Create(async()=>
            {
                try
                {
                    while(!invocation.IsAwaiterCompleted(awaiter))
                        await UniTask.Yield();
                }
                catch {}
                finally
                {
                    OnCompleted();
                }
            });
            // invocation.GetAwaiter(r).OnCompleted(OnCompleted);
        }
        else
        {
            OnCompleted();
        }
        return r;
    }
```
上例使用`UniTask.Create`创建了一个Timer，可以使用其他类似的方法，如自定义MonoBehaviour等。一旦`IsAwaiterCompleted`检查结束，立即执行自定义的`OnCompleted`方法。

因为Unity没有官方支持的Timer功能，基于“此库只做自己该做的”原则，这里只是给出提示。

更多使用方法参考附带的Sample工程

## Possible Problems


|Problem|Reason|Solution|
|:-:|:-|:-|
|文档示例中的异步方法无法打印完整|WebGL平台不支持多线程|文档中使用的是Task，改成UniTask或其他方式即可|
|注入时未搜索到标记的方法|`Managed Stripping Level`过高，Attribute被移除|降低Stripping Level或 [保留代码](https://docs.unity3d.com/Manual/ManagedCodeStripping.html)|



## How it works
UnityInjection在编译时织入，不用担心运行时兼容性

如何注入：

  * 运行时：在打包的Link阶段修改DLL，如此使Runtime生效
  * 编辑器时：~~菜单[Tools/bbbirder/inject for Editor]，手动使编辑器模式生效。~~ 通常自动生效

## Todo List
1. 更多Unity版本测试，有问题提ISSUE附Unity版本，或者PR。
2. 提供可选的混合模式：Editor下可以使用MonoHook更方便地注入。
3. **支持泛型**（有需要则提前实现）
