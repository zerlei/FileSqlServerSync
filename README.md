> 这是一个基于 asp.net c# 的发布工具。

```bash
# 此行命令差一个运行环境复制。bin/roslyn
msbuild .\HMES-H7-HNFYMF.WEB\HMES_H7_HNFYMF.WEB.csproj /t:ResolveReferences /t:Compile /t:_CopyWebApplication /p:Configuration=Release /p:WebProjectOutputDir=C:\publish /p:OutputPath=C:\publish\bin


# 此命令是一个完整的发布命令

msdeploy.exe -verb:sync -source:contentPath=D:\git\HMES-H7-HNFY\HMES-H7-HNFYMF\HMES-H7-HNFYMF.WEB -dest:contentPath=D:\git\HMES-H7-HNFY\HMES-H7-HNFYMF\release -disablerule:BackupRule
```

```plantuml
package 服务器 {

    component remoteserver
    note left : 1. windows server \n2. IIS部署
    component webtool
}
package 本地计算机 {
    component localserver
    note left : 1. windows 10/11 \n2. windows 服务
}

package asp.net [
.net 8
----
sqlite
----
http
---
websocket
]

package webpage [
    vue3
    ---
    naive-ui
]
asp.net --+ remoteserver
asp.net --+ localserver
webpage --+ webtool
```
