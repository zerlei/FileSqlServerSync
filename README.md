> 这是一个基于 asp.net c# 的发布工具。

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

```plantuml

entity DirInfo {
    String ServerPath
    String LocalPath
    Arr SpecialFiles
    Arr ExcludeFiles
}
entity FileInfo {
    DateTime mtime
    String Path
}
entity ConfigInfo {
    Arr<DirInfo> DirInfos
    String RemoteAddr
    String RemoteName
}
```

```plantuml
allowmixing
skinparam classAttributeIconSize 0
component sqlite
package remoteserver {
    class FilesInfoController {
        +Arr<FileInfo> GetFilesInfo(DirInfo dirinfo)
        +UploadFiles(FileInfo)
    }
    class SyncLogController {
        +GetSyncLog()
    }
    package SyncPersistence {
    }

    SyncPersistence --* SyncLogController
    SyncPersistence --* FilesInfoController

}
package localserver {
    class FilesConfigController {
        - GetLocalFilesInfo()
        - CompareLocalRemoteFiles()
        --
        + SetDirsConfig(Arr<DirInfo> dirs)
    }
    package ConfigPersistence {
    }
    ConfigPersistence --* FilesConfigController
}
FilesConfigController <--> FilesInfoController: 文件及信息传递
sqlite --* SyncPersistence
sqlite --* ConfigPersistence

Actor Devloper

Devloper --> FilesConfigController:调用

Devloper --> SyncLogController:查看同步信息
```
