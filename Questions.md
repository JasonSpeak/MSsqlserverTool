# 遇到的一些问题：
> - Q：动态生成的treeview需要根据item内容生成不同的contexmenu
>   - A:使用DataTrigger，根据binding数据的不同设置不同的contextmenu
> - Q：contextmenu是单独的控件，无法通过层级关系获取父控件
>   - A：[使用PlacementTarget，获取contextmenu打开时的相对位置。](https://yq.aliyun.com/articles/677949)
> - Q：MVVMLight中CommandParameter的使用
>   - A：[如何获取动态绑定控件](https://www.cnblogs.com/wzh2010/p/6607702.html)
> - Q：打开表格，需要点击表名动态生成ListView
>   - A：写一个`CommandBehavior`作为控件的依赖属性。
> - Q：控件之间的绑定
>   -A：使用RelativeSource动态查找逻辑树上的控件
> - Q：模板控件的样式不符合要求
>   - A：寻找模板控件样式的源码，将源码加入对应Window的`Window.Resource`中
> - Q：