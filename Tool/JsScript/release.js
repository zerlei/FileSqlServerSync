import chalk from "chalk";
import WebSocket from "ws";

//#region  ############################## 配置文件 ###################################

const LocalHost = "127.0.0.1";
const config = {
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
//#endregion

//#region  ############################## 打印函数 ###################################

let IsSuccess = false;
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
function PrintCloseNewLine(str) {
  if (IsSuccess) {
    PrintSuccessInNewLine(str);
  } else {
    PrintErrInNewLine(str);
  }
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
let ws = null;
function MsgCb(MsgIt) {
  if (MsgIt.Type == 2) {
    PrintGeneralInCurrentLine(MsgIt.Body);
  } else {
    if (MsgIt.Step <= 6) {
      PrintSuccessInNewLine(`(${MsgIt.Step}/6) ${MsgIt.Body}`);
      if (MsgIt.Step == 6) {
        if (MsgIt.Body == "发布完成！") {
          IsSuccess = true;
          ws.close();
        }
      }
    } else if (MsgIt == 7) {
      PrintErrInNewLine(MsgIt.Body);
    } else {
      PrintCloseNewLine("(关闭)" + MsgIt.Body);
    }
  }
}
//#endregion
async function connectWebSocket() {
  return new Promise((resolve, reject) => {
    const wsUrl = `ws://${LocalHost}:6818/websoc?Name=${config.Name}`;
    ws = new WebSocket(wsUrl);

    ws.onopen = (event) => {
      var starter = {
        Body: JSON.stringify(config),
        Type: 1,
        Step: 1,
      };
      //   console.warn("websocket connected!");
      ws.send(JSON.stringify(starter));
    };
    ws.onmessage = (event) => {
      // console.log(event.data);
      MsgCb(JSON.parse(event.data));
    };
    ws.onclose = (event) => {
      // console.warn(event)
      MsgCb({
        Type: 0,
        Step: 8,
        Body: event.reason,
      });
      // resolve()
    };
    ws.onerror = (e) => {
      // console.error(e);
      MsgCb({
        Type: 0,
        Body: "异常错误，查看 Console",
        Step: 7,
      });
      reject(err);
    };
  });
}
(async function main() {
  try {
    await connectWebSocket();
    // console.log('WebSocket has closed');
    // The script will wait here until the WebSocket connection is closed
  } catch (err) {
    console.error("Failed to connect or an error occurred:", err);
  }
})();
