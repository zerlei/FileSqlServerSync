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
  Name: "Test",
  RemoteUrl: "127.0.0.1:6819",
  RemotePwd: "t123",
  IsDeployDb: false,
  IsDeployProject: false,
  LocalProjectAbsolutePath:
    "D:/git/HMES-H7-HNFY/HMES-H7-HNFYMF/HMES-H7-HNFYMF.WEB",
  LocalRootPath: "D:/FileSyncTest/src",

  RemoteRootPath: "D:/FileSyncTest/dst",
  SrcDb: {
    ServerName: "172.16.12.2",
    DatebaseName: "HMES_H7_HNFYMF",
    User: "hmes-h7",
    Password: "Hmes-h7666",
    TrustServerCertificate: "True",
    SyncTablesData: [
      "dbo.sys_Button",
      "dbo.sys_Menu",
      "dbo.sys_Module",
      "dbo.sys_Page",
    ],
  },
  DstDb: {
    ServerName: "127.0.0.1",
    DatebaseName: "HMES_H7_HNFYMF",
    User: "sa",
    Password: "0",
    TrustServerCertificate: "True",
  },
  DirFileConfigs: [
    {
      DirPath: "/bin",
      Excludes: ["/roslyn", "/Views"],
    },
  ],
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
