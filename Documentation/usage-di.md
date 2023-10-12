# 依赖注入

依赖注入对于模块间解耦帮助很大。

## 基本用法

在类成员上使用`[SimpleDI]`标签，使其可被注入。

```csharp
public interface IFoo
{

}

public class FooA : IFoo
{

}

public class FooB : IFoo
{

}

public class ServiceA
{
    [SimpleDI] public IFoo foo {get;}
}

```

定义规则:

```csharp
void Start()
{
    // 使用FooA实现IFoo
    ServiceContainer.Bind<IFoo>().AsSingle(noLazy:true).To<FooA>();
}

```

## 支持注入的成员

支持以下成员：

+ 字段、属性
+ 静态成员、实例成员
+ 只读成员、可读可写成员

如：

```csharp
class Foo
{
    [SimpleDI] static IFoo foo;
    [SimpleDI] static IFoo foo { get; }
    [SimpleDI] static IFoo foo { get; set; }
    [SimpleDI] IFoo foo;
    [SimpleDI] IFoo foo { get; }
    [SimpleDI] IFoo foo { get; set; }
}

```

## 更多控制

### 限定DeclaringType

```csharp
void Init()
{
    // 使用FooA实现IFoo，只对ServiceA中的成员生效
    ServiceContainer.In<ServiceA>().Bind<IFoo>().AsSingle(noLazy:true).To<FooA>();
}

```

### 构造函数

```csharp
interface IFoo { }
class Foo : IFoo
{
    public Foo(string name, int age)
    {
        Console.WriteLine($"我的伙伴{name},{age}岁了");
    }
}

void Init()
{
    // output: 我的伙伴小黑儿,3岁了
    ServiceContainer.Bind<IFoo>().AsSingle(noLazy:true).To<Foo>("小黑儿",3); 
}

```

### 生命模式 ScopeMode

+ Single 单例（默认）
+ Transient 每次获取都是新实例

```csharp
    // 使用FooA实现IFoo
    ServiceContainer.Bind<IManager>().AsSingle().To<FooManager>();
    ServiceContainer.Bind<IFoo>().AsTransient().To<Foo>();
```

### 泛型递归

对于`IManager<IFoo,IBar>`会分别查找IManager,IFoo,IBar，并支持泛型约束。

```csharp
interface IFoo
{

}

interface IManager
{

}

class Foo : IFoo
{

}

class Manager : IManager
{

}

```

```csharp
void Init()
{
    ServiceContainer.Get<IManager<IFoo>>(); // returns Manager<Foo>
}
```
