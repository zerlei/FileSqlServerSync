using System.Text.Json;
using Common;
using LocalServer;
using RemoteServer;
using XUnit.Project.Attributes;

/*using Newtonsoft.Json;*/

namespace ServerTest;

public class PipeTest
{
    [Fact]
    public async void TestCase()
    {
        var p1 = new TestPipe(false, "1");
        var x = Task.Run(async () =>
        {
            var rs = p1.Work(
                (byte[] b) =>
                {
                    var msg = JsonSerializer.Deserialize<SyncMsg>(b);
                    if (msg.Body == "发布完成！")
                    {
                        p1.Close("正常退出！");
                    }
                    Console.WriteLine(b);
                    return true;
                }
            );
            await foreach (var r in rs)
            {
                Console.WriteLine(r);
            }
        });
        //await p1.Close("sdf");
        //await x;
        var p2 = new TestPipe(false, "2");
        p1.Other = p2;
        p2.Other = p1;
        var p3 = new TestPipe(true, "3");
        var p4 = new TestPipe(true, "4");
        p3.Other = p4;
        p4.Other = p3;
        LocalSyncServer.TempRootFile = "D:/FileSyncTest/stemp";
        RemoteSyncServer.SqlPackageAbPath = "C:\\Users\\ZHAOLEI\\.dotnet\\tools\\sqlpackage.exe";
        //LocalSyncServer.MsdeployAbPath =
        //    "C:\\Program Files\\IIS\\Microsoft Web Deploy V3\\msdeploy.exe";
        LocalSyncServer.SqlPackageAbPath = "C:\\Users\\ZHAOLEI\\.dotnet\\tools\\sqlpackage.exe";
        LocalSyncServer.MSBuildAbPath =
            "C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\MSBuild\\Current\\Bin\\amd64\\MSBuild.exe";
        RemoteSyncServer.TempRootFile = "D:/FileSyncTest/dtemp";
        RemoteSyncServerFactory.NamePwd = [new Tuple<string, string>("Test", "t123")];
        var lf = new LocalSyncServerFactory();
        var task1 = Task.Run(() =>
        {
            lf.CreateLocalSyncServer(p2, "Test", p3).RunSynchronously();
        });

        var rf = new RemoteSyncServerFactory();

        var task2 = Task.Run(async () =>
        {
            await rf.CreateRemoteSyncServer(p4, "Test");
        });
        TestPipe.syncServerFactory = rf;
        var starter = new SyncMsg
        {
            Body = JsonSerializer.Serialize(new PipeSeed().TestConfig),
            Type = SyncMsgType.General,
            Step = SyncProcessStep.Connect
        };
        await p1.SendMsg(starter);
        await x;
        if (p1.ErrResult != "正常退出！")
        {
            Assert.Fail(p1.ErrResult);
        }
    }
}
