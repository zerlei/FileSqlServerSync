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
        // var p1 = new TestPipe(false, "1");
        // var x = Task.Run(async () =>
        // {
        //     var rs = p1.Work(
        //         (byte[] b) =>
        //         {
        //             Console.WriteLine(b);
        //             return true;
        //         }
        //     );
        //     await foreach (var r in rs)
        //     {
        //         Console.WriteLine(r);
        //     }
        // });
        // //await p1.Close("sdf");
        // //await x;
        // var p2 = new TestPipe(false, "2");
        // p1.Other = p2;
        // p2.Other = p1;
        // var p3 = new TestPipe(true, "3");
        // var p4 = new TestPipe(true, "4");
        // p3.Other = p4;
        // p4.Other = p3;
        // RemoteSyncServerFactory.NamePwd = [new Tuple<string, string>("Test", "t123")];
        // var lf = new LocalSyncServerFactory();
        // lf.CreateLocalSyncServer(p2, "Test", p3);
        // var rf = new RemoteSyncServerFactory();
        // rf.CreateRemoteSyncServer(p4, "Test");
        // var starter = new SyncMsg
        // {
        //     Body = JsonSerializer.Serialize(new PipeSeed().TestConfig),
        //     Type = SyncMsgType.General,
        //     Step = SyncProcessStep.Connect
        // };
        // await p1.SendMsg(starter);
        // await x;
        // if (p1.ErrResult != null)
        // {
        //     Assert.Fail(p1.ErrResult);
        // }
    }
}
