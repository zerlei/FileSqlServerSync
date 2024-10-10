<script setup>
import MonacoEditor from 'monaco-editor-vue3';
import HistoryBtn from "./HistoryBtn.vue"
import { ref, onMounted, computed } from 'vue';
import stringifyObject from 'stringify-object';
const cacheConfig = ref({})
const options = ref({

  colorDecorators: true,
  lineHeight: 24,
  tabSize: 2,
})
const code = ref(`
config = {
  Name: "Test",
  RemoteUrl: "D:/FileSyncTest/dtemp",
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
function submit() {
  var config = {}
  try {
    eval(code.value)
    if ([undefined, null, ''].includes(config.Name)) {
      throw "缺少名称！"
    }
    cacheConfig.value[config.Name] = config
    updateStorage()
  }
  catch (e) {
    window.alert(e)
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

function publishCB(MsgIt) {

}
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
    </div>
  </div>


  <button style="margin-top: 20px;" @click="submit">发布</button>
</template>

<style scoped></style>
