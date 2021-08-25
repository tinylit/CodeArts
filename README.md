![CodeArts](http://oss.jschar.com/codearts.png 'Logo')

[![Gitee](https://gitee.com/static/images/logo-en.svg)](https://gitee.com/tinylit/codearts)
[![Github](http://oss.jschar.com/logo_github.png)](https://github.com/tinylit/codearts)

![GitHub](https://img.shields.io/github/license/tinylit/codearts.svg)
![language](https://img.shields.io/github/languages/top/tinylit/codearts.svg)
![codeSize](https://img.shields.io/github/languages/code-size/tinylit/codearts.svg)
![appveyor-ci](https://img.shields.io/appveyor/ci/tinylit/codearts.svg)
![AppVeyor tests (compact)](https://img.shields.io/appveyor/tests/tinylit/codearts.svg?compact_message)
[![GitHub issues](https://img.shields.io/github/issues-raw/tinylit/codearts)](../../issues)



### “CodeArts”是什么？

CodeArts 是一套简单、高效的轻量级框架（涵盖了类型转换、实体复制、实体映射、动态代理类，以及基于Linq分析实现的、支持分表和读写分离的ORM框架）。

### 如何安装？
First, [install NuGet](http://docs.nuget.org/docs/start-here/installing-nuget). Then, install [CodeArts](https://www.nuget.org/packages/CodeArts/) from the package manager console: 

```
PM> Install-Package CodeArts
```

NuGet 包
--------

| Package | NuGet | Downloads | Jane Says <kbd>Markdown</kbd> |
| ------- | ----- | --------- | --------- |
| CodeArts | [![CodeArts](https://img.shields.io/nuget/v/CodeArts.svg)](https://www.nuget.org/packages/CodeArts/) | ![Nuget](https://img.shields.io/nuget/dt/CodeArts) | Core universal design. |
| CodeArts.Middleware | [![CodeArts.Middleware](https://img.shields.io/nuget/v/CodeArts.Middleware.svg)](https://www.nuget.org/packages/CodeArts.Middleware/) | ![Nuget](https://img.shields.io/nuget/dt/CodeArts.Middleware) | [IOC middleware.](./CodeArts.Middleware.md) |
| CodeArts.Caching | [![CodeArts.Caching](https://img.shields.io/nuget/v/CodeArts.Caching.svg)](https://www.nuget.org/packages/CodeArts.Caching/) | ![Nuget](https://img.shields.io/nuget/dt/CodeArts.Caching) | [Caching rules.](./CodeArts.Caching.md) |
| CodeArts.Casting | [![CodeArts.Casting](https://img.shields.io/nuget/v/CodeArts.Casting.svg)](https://www.nuget.org/packages/CodeArts.Casting/) | ![Nuget](https://img.shields.io/nuget/dt/CodeArts.Casting) | [Type conversion, cloning, mapping.](./CodeArts.Casting.md) |
| CodeArts.Configuration | [![CodeArts.Configuration](https://img.shields.io/nuget/v/CodeArts.Configuration.svg)](https://www.nuget.org/packages/CodeArts.Configuration/) | ![Nuget](https://img.shields.io/nuget/dt/CodeArts.Configuration) | [Read configuration file.](./CodeArts.Configuration.md) |
| CodeArts.Json | [![CodeArts.Json](https://img.shields.io/nuget/v/CodeArts.Json.svg)](https://www.nuget.org/packages/CodeArts.Json/) | ![Nuget](https://img.shields.io/nuget/dt/Codearts.Json) | [JSON read and write processing.](./CodeArts.Json.md) |
| CodeArts.Net | [![CodeArts.Net](https://img.shields.io/nuget/v/CodeArts.Net.svg)](https://www.nuget.org/packages/CodeArts.Net/) | ![Nuget](https://img.shields.io/nuget/dt/CodeArts.Net) | [Request component of HTTP/HTTPS.](./CodeArts.Net.md) |
| CodeArts.Emit | [![CodeArts.Emit](https://img.shields.io/nuget/v/CodeArts.Emit.svg)](https://www.nuget.org/packages/CodeArts.Emit/) | ![Nuget](https://img.shields.io/nuget/dt/CodeArts.Emit) | Abstract Syntax Tree(AST). |
| CodeArts.Mvc | [![CodeArts.Mvc](https://img.shields.io/nuget/v/CodeArts.Mvc.svg)](https://www.nuget.org/packages/CodeArts.Mvc/) | ![Nuget](https://img.shields.io/nuget/dt/CodeArts.Mvc) | [Model View Controller(MVC).](./CodeArts.Mvc.md) |
| CodeArts.Db | [![CodeArts.Db](https://img.shields.io/nuget/v/CodeArts.Db.svg)](https://www.nuget.org/packages/CodeArts.Db/) | ![Nuget](https://img.shields.io/nuget/dt/CodeArts.Db) | [Database operation base library.](./CodeArts.Db.md) |
| CodeArts.Db.Lts | [![CodeArts.Db.Lts](https://img.shields.io/nuget/v/CodeArts.Db.Lts.svg)](https://www.nuget.org/packages/CodeArts.Db.Lts/) | ![Nuget](https://img.shields.io/nuget/dt/CodeArts.Db.Lts) | [Independent research and development and long term maintenance of ORM.](./CodeArts.Db.Lts.md) |
| CodeArts.Db.EntityFramework | [![CodeArts.Db.EntityFramework](https://img.shields.io/nuget/v/CodeArts.Db.EntityFramework.svg)](https://www.nuget.org/packages/CodeArts.Db.EntityFramework/) | ![Nuget](https://img.shields.io/nuget/dt/CodeArts.Db.EntityFramework) | [EF/EFCore simple package.](./CodeArts.Db.EntityFramework.md) |
| CodeArts.Db.Linq2Sql | [![CodeArts.Db.Linq2Sql](https://img.shields.io/nuget/v/CodeArts.Db.Linq2Sql.svg)](https://www.nuget.org/packages/CodeArts.Db.Linq2Sql/) | ![Nuget](https://img.shields.io/nuget/dt/CodeArts.Db.Linq2Sql) | [Linq to SQL.](./CodeArts.Db.Linq2Sql.md) |

### 引包即用？

* 引包即用是指，安装 `NuGet` 包后，自动注入配置信息。
* 在启动方法中添加如下代码即可：
``` csharp
using (var startup = new XStartup())
{
    startup.DoStartup();
}
```

### 单例。

* 作为单例基类。

  ```c#
  public class ASingleton : Singleton<ASingleton> {
      private ASingleton(){ }
  }
  
  ASingleton singleton = ASingleton.Instance;
  ```

* 作为单例使用。

  ```c#
  public class BSingleton {   
  }
  
  BSingleton singleton = Singleton<BSingleton>.Instance
  ```

* 绝对单例。

  ```c#
  public class CSingleton : Singleton<CSingleton> {
      private CSingleton(){ }
  }
  
  CSingleton singleton1 = CSingleton.Instance;
  CSingleton singleton2 = Singleton<CSingleton>.Instance; // 与“singleton1”是同一实例。
  ```

### 服务池。

* TryAddSingleton：添加单例实现。

* Singleton：获取单例。

* 单例实现：

  - 单例实现（一）。

    - 添加默认支持的单例实现。

    ```c#
    RuntimeServPools.TryAddSingleton<A,B>(); //=> true.
    ```

    - 在未使用A的实现前，可以刷新单例实现支持。

    ```c#
    RuntimeServPools.TryAddSingleton<A,C>(); //=> true;
    RuntimeServPools.TryAddSingleton<A>(new C()); //=> true;
    ```

  - 单例实现（二）。

    - 添加实例或工厂支持的单例实现。

    ```c#
    RuntimeServPools.TryAddSingleton<A>(new B()); //=> true.
    ```

    - 在未使用A的实现前，可以被实例或工厂刷新单例实现支持，默认支持方式不被生效。

    ```c#
    RuntimeServPools.TryAddSingleton<A,C>(); //=> false;
    RuntimeServPools.TryAddSingleton<A>(new C()); //=> true;
    ```

* 单例使用：

  - 单例使用（一）。

    ```c#
    A a = RuntimeServPools.Singleton<A>();
    ```

    未提前注入单例实现，会直接抛出`NotImplementedException`异常。

  - 单例使用（二）。

    ```c#
    A a = RuntimeServPools.Singleton<A,B>();
    ```

    未提前注入单例实现，会尝试创建`B`实例。

- 说明：

  - TryAddSingleton&lt;T&gt;：使用实例时，使用【公共/非公共】无参构造函数创建实例。

  - TryAddSingleton&lt;T1,T2&gt;：使用实例时，尽量使用参数更多且被支持的公共构造函数创建实例。

    ```c#
    public class A {
    }
    public class B {
        private readonly A a;
        public B() : this(new A()){ }
        Public B(A a){ this.a = a ?? throw new ArgumentNullException(nameof(a)); }
    }
    ```

    使用单例时，未注入A的单例实现，使用无参构造函数生成实例。

    使用单例时，已注入A的单例实现，使用参数`A`的造函数生成实例。

### 命名规范。

* 命名方式。

  ```c#
  /// <summary> 命名规范。 </summary>
  public enum NamingType
  {
      /// <summary> 默认命名(原样/业务自定义)。 </summary>
      Normal = 0,
  
      /// <summary> 驼峰命名,如：userName。 </summary>
      CamelCase = 1,
  
      /// <summary> url命名,如：user_name，注：反序列化时也需要指明。 </summary>
      UrlCase = 2,
  
      /// <summary> 帕斯卡命名,如：UserName。 </summary>
      PascalCase = 3
  }
  ```

* 命名标记。

  ```c#
  /// <summary>
  /// 命名特性。
  /// </summary>
  [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
  public sealed class NamingAttribute : Attribute
  {
      /// <summary>
      /// 名称。
      /// </summary>
      public string Name { get; set; }
  
      /// <summary>
      /// 命名规范。
      /// </summary>
      public NamingType NamingType { get; set; }
  
      /// <summary>
      /// 构造函数。
      /// </summary>
      /// <param name="name">名称。</param>
      public NamingAttribute(string name)
      {
          Name = name;
      }
  
      /// <summary>
      /// 构造函数。
      /// </summary>
      /// <param name="namingType">名称风格。</param>
      public NamingAttribute(NamingType namingType)
      {
          NamingType = namingType;
      }
      /// <summary>
      /// 构造函数。
      /// </summary>
      /// <param name="name">名称。</param>
      /// <param name="namingType">名称风格。</param>
      public NamingAttribute(string name, NamingType namingType) : this(name)
      {
          NamingType = namingType;
      }
  }
  ```

### 命名转换。

* 指定命名方式。

  ```c#
  string named = "name".ToNamingCase(NamingType.CamelCase);
  ```

* 特定命名方式。

  - 帕斯卡命名（又称大驼峰）。

    ```c#
    string named = "name".ToPascalCase();
    ```

  - 驼峰命名。

    ```c#
    string named = "name".ToCamelCase();
    ```

  - Url命名。

    ```c#
    string named = "name".ToUrlCase();
    ```

### 读取配置文件。

* 普通方式。

  ```c#
  T value = "key".Config<T>(); // 未找到时，返回指定类型的默认值。
  ```

  ```c#
  T value = "key".Config(defaultValue); // 未找到时，返回defaultValue。
  ```

* 配置变更自动同步方式。

  - 实现以下接口。

    ```c#
    /// <summary>
    /// 配置变更监听能力。
    /// </summary>
    public interface IConfigable<TSelf> where TSelf : class, IConfigable<TSelf>
    {
    	/// <summary>
    	/// 监听到变更后的新数据。
    	/// </summary>
    	/// <param name="changedValue">变更后的数据。</param>
    	void SaveChanges(TSelf changedValue);
    }
    ```

  - 使用方式。

    ```c#
    T value = "key".Config<T>(); // 未找到时，返回:null。
    ```

### 字符串语法糖。

```c#
string value = "{a + b}".PropSugar(new { A = 1, B = 2 }, NamingType.CamelCase); //=> value = "3"。
```

* 语法说明：

  - 空运算符：A?B、A ?? B

    当A为`null`时，返回B，否则返回A。

  - 合并运算符：A+B

    当A和B可以参与运算时，返回运算结果。否则转成字符串拼接。

  - 试空合并运算符：A?+B

    当A为`null`时，返回B，否则按照【合并运算符】计算A+B的结果。

  - 可支持任意组合，从左到右依次计算（不支持小括号）。

### 可空能力（非`null`空实例）。

* Nullable&lt;T&gt;：`T`的默认值。

* 值类型：默认值。

* 字符串类型：`string.Empty`。

* 自定义空实例实现：`Emptyable.Register`。

  - 普通类型。

    ```c#
    Emptyable.Register<Version>(new Version());
    ```

  - 继承或实现关系。

    ```c#
    public class A{}
    public class B : A {}
    
    Emptyable.Register<A,B>();
    ```

  - 泛型声明类型。

    ```c#
    public class A<T>{}
    public class B<T> : A<T> {}
    
    Emptyable.Register(typeof(A<>),typeof(B<>));
    ```

* 其它：调用参数最多的构造函数生成默认值，确保构造函数参数相关内容不会为`null`。

  - 可选参数不为`null`时，直接使用可选参数默认值。
  - 生成可空实例。

* 不能生成非`null`空实例时，抛出异常。

### 标星历程图。

[![Stargazers over time](https://starchart.cc/tinylit/CodeArts.svg)](https://starchart.cc/tinylit/CodeArts)
