# UnityInjection

Unity注入模块，可以运行时改变被注入函数实现。

![mono support](http://img.shields.io/badge/Mono-support-green)
![il2cpp support](http://img.shields.io/badge/IL2CPP-support-green)
![GitHub last commit](http://img.shields.io/github/last-commit/labbbirder/UnityInjection)
![GitHub package.json version](http://img.shields.io/github/package-json/v/labbbirder/UnityInjection)

Verified Verisons:

|Unity 2021.3.x|Unity 2022.3.x|
|:-:|:-:|
|:heavy_check_mark:|:heavy_check_mark:|

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

#### install via git url

step 1. 安装依赖库：[DirectRetrieveAttribute](https://github.com/labbbirder/DirectRetrieveAttribute#安装)

step 2. 通过git url安装

#### install via openupm (recommend)

execute command line：

```bash
openupm add com.bbbirder.injection
```

### Basic Usage

一个修改`Debug.Log`的例子

```csharp
using com.bbbirder.injection;
using UnityEngine;

// this illustration shows how to hook `Debug.Log`
public class FirstPatch:IInjection
{
    // the field to be overwrited to original method
    static Action<object> RawLog;

    static void Log(object msg){
        return RawLog.Invoke("[msg] "+msg); 
    }

    internal class MethodReplacer : IInjection
    {
        // implement method: ProvideInjections
        public IEnumerable<InjectionInfo> ProvideInjections()
        {
            yield return InjectionInfo.Create<Action<object>>(
                Debug.Log,      // replace Debug.Log
                FirstPatch.Log, // with FirstPatch.Log
                f => FirstPatch.RawLog = f // save origin method to FirstPatch.RawLog
            );
        }
    }
}

```

`FixerAttribute`接收3个参数：

  1. 目标类型
  2. 目标方法名称
  3. 用于保存原方法的函数成员名称

初始化的时候调用：

```csharp
FixHelper.InstallAll();// 查找所有注入标记，并使生效
```

测试成果：

```csharp
Debug.Log("hello"); //output: [msg] hello
```

## 更多用法

[装饰器](./Documentation/usage-decorator.md)

[数据代理](./Documentation/usage-proxy.md)

[依赖注入](./Documentation/usage-di.md)

更多使用方法参考附带的Sample工程

## Possible Problems

|Problem|Reason|Solution|
|:-:|:-|:-|
|文档示例中的异步方法无法打印完整|WebGL平台不支持多线程|文档中使用的是Task，改成UniTask或其他方式即可|
|注入时未搜索到标记的方法|`Managed Stripping Level`过高，Attribute被移除|降低Stripping Level或 [保留代码](https://docs.unity3d.com/Manual/ManagedCodeStripping.html)|
|注入时报`UnauthorizedAccessException`或`cannot access file`|文件访问权限不够|管理员运行 或 修改目标文件夹的安全设置（属性-安全-编辑，添加当前用户的完全控制）|
|打包时报`the same key has already been added. Key: mscorlib`|不支持的Unity版本，程序集依赖树包含多个不同版本的mscorlib|暂不提供解决方案，可自行修改类型引入部分逻辑，如果这个问题影响的人多则解决之|

## How it works

UnityInjection在编译时织入，不用担心运行时兼容性

织入时机：

* 运行时：在打包的Link阶段修改DLL，如此使Runtime生效
* 编辑器时：Domain Reload

## Todo List

1. 更多Unity版本测试，有问题提ISSUE附Unity版本，或者PR。
2. 提供可选的混合模式：Editor下可以使用MonoHook更方便地注入。
3. **支持泛型**（有需要则提前实现）
