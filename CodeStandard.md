- [Lonwin C# Wpf Code Standard](#lonwin-c-wpf-code-standard)
  - [命名标准](#%e5%91%bd%e5%90%8d%e6%a0%87%e5%87%86)
  - [分层标准](#%e5%88%86%e5%b1%82%e6%a0%87%e5%87%86)
  - [编码要求](#%e7%bc%96%e7%a0%81%e8%a6%81%e6%b1%82)

---
## Lonwin C# Wpf Code Standard
### 命名标准   
> - private属性：`_privateProperty`
> - public属性：`PublicProperty`
> - 类名：`ClassName`
> - 接口：`IInterfaceName`
> - 变量：`variableName` --**尽量使用var初始化**
> - 集合：`collections` --**使用复数**
### 分层标准
> - 数据处理尽量放入Model，包括数据类定义，数据类操作，数据库操作，异常的获取处理抛出等。
> - ViewModel：包括对外暴露数据的初始化，调用Model层的方法对数据处理，封装，通过command处理view层的互动，处理view层的绑定属性。
> - view：只包含必要的控件和布局。style使用resource文件放入Resources文件夹，view层调用即可。
> - image、style、icon等资源文件放入Resources文件夹。
> - Converters辅助类单独放入一个文件夹
### 编码要求
> - public方法必须校验参数，并且不可以与其他public方法有关联