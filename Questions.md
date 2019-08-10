# 遇到的一些问题：
> - Q：动态生成的treeview需要根据item内容生成不同的contexmenu
>   - A:使用DataTrigger，根据binding数据的不同设置不同的contextmenu
> - Q：contextmenu是单独的控件，无法通过层级关系获取父控件
>   - A：使用PlacementTarget，获取contextmenu打开时的相对位置。[链接](https://yq.aliyun.com/articles/677949)
> - Q：MVVMLight中CommandParameter的使用
>   - A：[链接](https://www.cnblogs.com/wzh2010/p/6607702.html)