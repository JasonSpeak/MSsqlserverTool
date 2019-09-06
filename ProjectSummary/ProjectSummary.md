---
presentation:
  # === 可选的主题 ===
  # "beige.css"
  # "black.css"
  # "blood.css"
  # "league.css"
  # "moon.css"
  # "night.css"
  # "serif.css"
  # "simple.css"
  # "sky.css"
  # "solarized.css"
  # "white.css"
  # "none.css"  
  theme: serif.css

  # Display controls in the bottom right corner
  controls: true

  # Display a presentation progress bar
  progress: true

  # Display the page number of the current slide
  slideNumber: true

  # Push each slide change to the browser history
  history: false

  # Enable keyboard shortcuts for navigation
  keyboard: true

  # Enable the slide overview mode
  overview: true

  # Vertical centering of slides
  center: false

  # Enable slide navigation via mouse wheel
  mouseWheel: true

  # Hides the address bar on mobile devices
  hideAddressBar: false

  # Opens links in an iframe preview overlay
  previewLinks: false

  # Transition style
  transition: 'fade' # none/fade/slide/convex/concave/zoom

  # Transition speed
  transitionSpeed: 'default' # default/fast/slow

  # Transition style for full page slide backgrounds
  backgroundTransition: 'fade' # none/fade/slide/convex/concave/zoom
---

<!-- slide -->
## 数据库工具开发总结
<small>石晴蔚</small>

<!-- slide -->
## 项目概况
### 项目需求
> **实现轻量的数据库交互工具，能够进行常用的数据操作,并在开发过程中锻炼实际开发能力**

<!-- slide -->
 - 功能需求：
   1. 直接连接本地的SqlServer
   2. 获取本地SqlServer中的数据库列表，并屏蔽系统数据库
   3. 备份数据库
   4. 从备份文件恢复数据库
   5. 查看数据表中的数据
   6. 对数据表中的数据进行增/删/改
   7. 完善的日志记录

<!-- slide -->
## 开发过程遇到的问题

<!-- slide -->
### 全屏溢出问题
问题描述：当需要自定义标题栏时，设置Window的两个属性可以屏蔽Wpf应用自带的标题栏
```xml
WindowStyle="None"
AllowsTransparency="True"
```
这样的话，在该应用全屏的时候就会出现溢出窗口的情况。

<!-- slide vertical=true-->
@import "Resources/Images/OverScreen.png" {title="OverScreen"}

<!-- slide vertical=true-->
这个问题很容易复现，根据Stackoverflow上的问题，[这个问题](https://stackoverflow.com/questions/2092782/borderless-window-application-takes-up-more-space-than-my-screen-resolution)已经存在了9年了
---
最终使用trigger解决了问题
```xaml
<Style.Triggers>
            <DataTrigger Binding="{Binding RelativeSource={RelativeSource Self},Path=WindowState}" Value="{x:Static WindowState.Maximized}">
                <Setter Property="BorderThickness" Value="8"/>
            </DataTrigger>
        </Style.Triggers>
```

<!-- slide -->
style


