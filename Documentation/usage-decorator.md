# 装饰器

值得一提的是，装饰器的实现有以下优势：

* 调用是0GC，低开销的
* 实现只需要指定一个`DecoratorAttribute`
* 支持装饰异步函数

## 例子

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
        FixHelper.InstallAll();
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

## 异步方法补充说明

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
