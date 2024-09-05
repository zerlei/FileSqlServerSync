import chalk from "chalk";
import WebSocket from "ws";


//#region  ############################## 配置文件 ###################################

//这是个例子，请在`config`中写你的配置
const example_config = {
  //发布的项目名称，它的目的是为了防止有两个人同时发布一个项目，和便于排查发布历史记录
  Name: "",
  //发布服务器的地址
  RemoteUrl: "http://192.168.1.100:8067",
    //源SqlServer数据库的链接字符串，一般是开发或者测试数据库，**此数据库的ip是相对于运行此脚本的机器**
  SrcDbConnection: "",
    //目的SqlServer数据库的链接字符串，一般是正式环境数据库，**此数据库的ip是相对于运行RemoteServer的机器**
  DstDbConnection: "",

  //发布数据库时，只同步结构。此数组中的表，将会连数据也一起同步
  SyncDataTables:[],
  //源文件目录地址，是要发布的文件根目录，它是绝对路径，!执行发布时将发布到这个目录!
  LocalRootPath: "",
  //目标文件目录地址，也就是部署服务的机器上的项目文件根目录，它是绝对路径
  RemoteRootPath: "",

 //根目录下的子目录，分子目录是为了针对不同的目录有不同的发布策略，它是相对路径，即相对于LocalRootPath和RemoteRootPath，文件数据依此进行
  DirFileConfigs: [
    {
    //子目录的相对路径
      DirPath: "",
      //排除的文件或目录，它是相对路径，相对于！！！LocalRootPath和RemoteRootPath！！！
      Excludes: [],
      //只追踪文件或目录，它是相对路径，相对于！！！LocalRootPath和RemoteRootPath！！！，它的优先级最高，如果你指定了它的值，Excludes将会失效
      CherryPicks: [],
    },
  ],
};
const config = {};
//#endregion

//#region  ############################## 打印函数 ###################################

/**
 * 在新行打印错误信息
 */
function PrintErrInNewLine(str) {
  process.stdout.write("\n");
  var chunk = chalk["red"]("[错误]: ");
  process.stdout.write(chunk);
  process.stdout.write(str);
}

/**
 * 在新行打印成功信息
 */
function PrintSuccessInNewLine(str) {
  process.stdout.write("\n");
  var chunk = chalk["green"]("[成功]: ");
  process.stdout.write(chunk);
  process.stdout.write(str);
}

/**
 * 在新行打印一般信息
 */
function PrintGeneralInNewLine(str) {
  process.stdout.write("\n");
  process.stdout.write(str);
}
/**
 * 在当前行打印一般信息，打印此行信息之前会清除当前行
 */
function PrintGeneralInCurrentLine(str) {
  process.stdout.write(`\r${str}`);
}

//#endregion

//#region ############################### work #############################################

/**
 * 1-n. localServer 工作，此处只展示信息
 */

//这个是固定死的
const wsUrl = `ws://127.0.0.1:4538/websoc?Name=${config.Name}`;
const ws = new WebSocket(wsUrl);

ws.on('open', () => {
    //上传配置
    ws.send(JSON.stringify(config),(err)=>{
        console.log(err)
        ws.close()
    })

});

ws.on('message', (message) => {
    var msg = message.toString('utf8')
    DealMsg(msg)
});

ws.on('close', () => {
});

function DealMsg(str) {
    var msg = JSON.parse(str)
    if(msg.IsSuccess) {

    } else {
        PrintErrInNewLine(msg.Body)
        ws.close()
    }

}
//#endregion
