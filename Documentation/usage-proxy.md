# 数据代理

下面的代码演示实现一个数据代理：

```csharp
public class Pat : ProxyData // 需要继承ProxyData
{
    public string name { get; set; }
    public int age { get; set; }
}

var pet = new Pet();
pet.OnSetProperty(key=>{
    print($"set {key}");
});
pet.OnGetProperty(key=>{
    print($"get {key}");
});

pet.age += 1;

/*
output:
    get age
    set age
*/

```

有时你需要判断代理类是否被正确注入：

```csharp
print(pet.IsFixed()); // false
FixHelper.InstallAll();
print(pet.IsFixed()); // true
```
