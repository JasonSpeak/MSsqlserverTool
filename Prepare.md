# 需求
> 1. 打开工具直接登录本地MSsql服务器
> 1. 数据库列表中屏蔽系统数据库
> 1. 数据库列表中显示用户创建的数据库
> 1. 可执行恢复备份操作 
> 1. 选中某个数据库可执行备份该数据库的操作（可选输出路径）
> 1. 双击（或其他操作）某个数据库显示该数据库的所有表
> 1. 双击（或其他操作）某张数据表显示该表数据（分页），使用列表显示
> 1. 可对表数据执行增删查改（在界面中直接修改）
> 1. 使用NLog生成日志

# 开发流程
> **数据库操作使用sqlconnection,不能解决的问题考虑sqlcmd**
> - 第一阶段：
>   - [X] 完成展示数据库列表功能（屏蔽系统数据库）
> - 第二阶段：
>   - [X] 完成数据库备份功能 
>   - [ ] 完成恢复备份功能
> - 第三阶段：
>   - [ ] 完成数据表显示功能
>   - [ ] 完成数据显示功能
> - 第四阶段：
>   - [ ] 完成修改数据功能

# 难点
> - sql服务的各种问题
> - 显示表内数据
> - 对数据进行增删查改
> - 

# 要用到的sql命令
> - 查询数据库列表
>   - select name from sysdatabases
> - 备份数据库   
    backup database database_name to disk='E:\backup\database_name.bak'
> - 恢复数据库  
>   - 先查询数据库是否存在，存在就删除  
    select [Name] from [sysdatabases] 
    存在的话就执行drop语句：
    drop database database_name
    可以再一次执行select语句，看看该数据库是否已经被删除了  
>   - 恢复数据库
    执行以下语句：  
    restore database database_name from disk='D:\backup\database_name.bak'  
    with  
    move 'database_name' to 'D:\Program Files\Microsoft SQL Server\MSSQL11.SQLEXPRESS\MSSQL\DATA\database_name.mdf',  
    move 'database_name_log' to 'D:\Program Files\Microsoft SQL   Server\MSSQL11.SQLEXPRESS\MSSQL\DATA\database_name_log.ldf'

# 注意点
> - 备份数据库可选择输出路径
> - 恢复数据库可选择备份文件，根据备份文件名称决定恢复的数据库名称