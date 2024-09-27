<script setup>
import MonacoEditor from 'monaco-editor-vue3';
import { ref } from 'vue';
const options = ref({

  colorDecorators: true,
  lineHeight: 24,
  tabSize: 2,
})
// let config = {
//   x:10,
//   y:20
// }
const code = ref(`
config = {
  x:10,
  y:20
}
`)
function submit() {
  var config = {}
  // const config =  eval(code.value)
  eval(code.value)
  console.log(config)
}
</script>

<template>
  <h3>发布工具</h3>
  <div>发布历史</div>
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
