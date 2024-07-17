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
