<script setup>
import MonacoEditor from 'monaco-editor-vue3';
import HistoryBtn from "./HistoryBtn.vue"
import { ref, onMounted, computed } from 'vue';
import stringifyObject from 'stringify-object';
import ConnectPipe from './connect.js'
const cacheConfig = ref({})
const options = ref({

  colorDecorators: true,
  lineHeight: 24,
  tabSize: 2,
})

let Pipe = null
const Msgs = ref([])
const code = ref(`
config = {
  //发布的名称，每个项目具有唯一的一个名称
  Name: "FYMF",
  RemotePwd: "FYMF",
  //远程服务器地址，也就是发布的目的地，它是正式环境
  RemoteUrl: "127.0.0.1:8007",
  //是否发布数据库 sqlserver
  IsDeployDb: true,
  //是否发布前重新构建项目
  IsDeployProject: false,
  //项目地址
  LocalProjectAbsolutePath:
    "D:/git/HMES-H7-HNFY/HMES-H7-HNFYMF/HMES-H7-HNFYMF.WEB",
  //源文件目录地址，是要发布的文件根目录，它是绝对路径，!执行发布时将发布到这个目录!
  LocalRootPath: "D:/FileSyncTest/src",
  //目标文件目录地址，也就是部署服务的机器上的项目文件根目录，它是绝对路径
  RemoteRootPath: "D:/FYMF",
  //源数据库配置 SqlServer,将会同步数据库的结构
  SrcDb: {
    //Host
    ServerName: "172.16.12.2",
    //数据库名
    DatabaseName: "HMES_H7_HNFYMF",
    User: "hmes-h7",
    Password: "Hmes-h7666",
    //是否信任服务器证书
    TrustServerCertificate: "True",
    //同步的数据，这些数据将会同步
    SyncTablesData: [
      "dbo.sys_Button",
      "dbo.sys_Menu",
      "dbo.sys_Module",
      "dbo.sys_Page",
    ],
  },
  //目标数据库配置 sqlserver
  DstDb: {
    ServerName: "127.0.0.1",
    DatabaseName: "HMES_H7_HNFYMF",
    User: "sa",
    Password: "Yuanmo520...",
    TrustServerCertificate: "True",
  },
  //子目录配置，每个子目录都有自己不同的发布策略，它是相对路径，即相对于LocalRootPath和RemoteRootPath(注意 '/'，这将拼成一个完整的路径)，文件数据依此进行,
  DirFileConfigs: [
    {
      DirPath: "/bin",
      //排除的文件或目录，它是相对路径，相对于！！！LocalRootPath和RemoteRootPath！！！
      Excludes: ["/roslyn", "/Views"],
      //只追踪文件或目录，它是相对路径，相对于！！！LocalRootPath和RemoteRootPath！！！，它的优先级最高，如果你指定了它的值，Excludes将会失效
      // CherryPicks:[]
    },
  ],
  ExecProcesses: [],
  //  ExecProcesses:[
  //   {
  //     // 参数
  //     Argumnets:"ls",
  //     //  执行命令位置
  //     FileName:"powershell",
  //     // 相关步骤开始之前（B）或之后 (A)
  //     StepBeforeOrAfter:"A",
  //     // 本地（L）或远程 (R) 执行
  //     ExecInLocalOrServer:"L",
  //     // 步骤 1. 连接远程 2. 发布项目 3. 文件对比 4. 提取sqlserver 5. 打包上传 6. 发布
  //     Step:1
  //   }
  //  ]
};
`)
var CStatus = ref('None')
function publishCB(MsgIt) {

  if (MsgIt.Type == 2) {
    Msgs.value[Msgs.value.length - 1] = MsgIt
  } else {
    Msgs.value.push(MsgIt)
  }
  if (MsgIt.Step == 6) {
    if (MsgIt.Body == "发布完成！") {
      CStatus.value = 'Success'
      Pipe.ClosePipe()
      dialogShow("正确：发布完成！")
    }
  }
  if (MsgIt.Step == 8) {
    if (CStatus.value != "Success") {
      dialogShow("失败：请查看错误信息！")
    }
    CStatus.value = "None"
  }
}
function submit() {
  Msgs.value = []
  var config = {}
  try {
    eval(code.value)
    if ([undefined, null, ''].includes(config.Name)) {
      throw "缺少名称！"
    }
    cacheConfig.value[config.Name] = config
    updateStorage()
    Pipe = new ConnectPipe()
    Pipe.OpenPipe(config, publishCB)
    CStatus.value = "Process"

  }
  catch (e) {
    dialogShow(e)
  }
}
function onLoad(name) {

  const pretty = stringifyObject(cacheConfig.value[name], {
    indent: '  ',
    singleQuotes: false
  });
  code.value = "\nconfig = " + pretty
}
const history = computed(() => {
  return Object.keys(cacheConfig.value)
})

function onDel(name) {
  delete cacheConfig.value[name]
  updateStorage()
}
function updateStorage() {
  localStorage.setItem('config', JSON.stringify(cacheConfig.value))
}


onMounted(() => {
  var cacheConfigStr = localStorage.getItem('config')
  if (cacheConfigStr) {
    cacheConfig.value = JSON.parse(cacheConfigStr)
  }
})

const dMsg = ref('') 
function dialogClose() {
  document.getElementById('dialog').close()
}
function dialogShow(msg) {
  dMsg.value = msg
  document.getElementById('dialog').showModal()
}
</script>

<template>
  <h3>发布工具</h3>
  <div>
    <HistoryBtn :name="name" @load="onLoad" @del="onDel" v-for="name in history" />
  </div>
  <div style="display: flex;">

    <MonacoEditor theme="vs-dark" :options="options" language="javascript" :width="800" :height="700"
      v-model:value="code"></MonacoEditor>
    <div style="width: 800px;height: 700px;background-color: #1e1e1e;">
      发布日志

      <p style="text-align: left;border: 1px solid gray;margin: 5px;" v-for="msg in Msgs">
        <span :style="{ width: '100px', color: msg.Step > 6 ? 'red' : 'green' }">[{{ msg.Step
          > 6 ? msg.Step > 7 ? "关闭" : "错误" : `${msg.Step}/${6}`}}]</span>
        <span style="margin-left: 5px ;">{{ msg.Body }}</span>
      </p>
    </div>
  </div>
  <dialog id="dialog">
    <p>{{ dMsg }}</p>
    <button @click="dialogClose">关闭</button>
  </dialog>


  <button :disabled="CStatus != 'None'" style="margin-top: 20px;" @click="submit">发布</button>
</template>

<style scoped></style>
