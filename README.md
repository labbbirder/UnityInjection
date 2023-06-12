# unityinject
Unity注入模块，可以运行时改变被注入函数实现。

![mono support](https://img.shields.io/badge/Mono-support-green)
![il2cpp support](https://img.shields.io/badge/IL2CPP-support-green)

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

## Usage
一个修改`VisualElement.FindAncestorUserData`的例子
```csharp
// this illustration shows how to hook `VisualElement.FindAncestorUserData`
public class FirstPatch
{
    // the field to be overwrited by original method
    static Func<VisualElement,object> RawFindAncestorUserData;

    [Injection(typeof(VisualElement),"FindAncestorUserData",nameof(RawFindAncestorUserData))]
    static object FindAncestorUserData(VisualElement ve){
        Debug.Log("Hack Me!!!");
        return RawFindAncestorUserData.Invoke(ve); 
    }
}

```
`InjectionAttribute`接收3个参数：
  1. 目标类型
  2. 目标方法名称
  3. 用于保存原方法的函数成员名称

初始化的时候调用：
```csharp
FixHelper.Install();// 查找所有注入标记，并使生效
```
测试成果：
```csharp
new VisualElement().FindAncestorUserData(); //output: Hack Me!!!
```
## How it works
如何注入：

  * 运行时：在打包的Link阶段修改DLL，如此使Runtime生效
  * 编辑器时：菜单[Tools/bbbirder/inject for Editor]，手动使编辑器模式生效。
