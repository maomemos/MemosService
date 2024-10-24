# MAOJI Service

[Memos](https://github.com/maomemos/Memos) 的后端部分。

# 环境依赖

.NET8.0 运行时。

# 本地开发

在程序包管理器控制台运行下列语句

```powershell
# 初始化数据库
ADD-MIGRATION InitialCreate
UPDATE-DATABASE
```

3. 配置 `appsettings.json`内的信息

配置包括跨域信息、JWT 生成配置、Email 等信息。