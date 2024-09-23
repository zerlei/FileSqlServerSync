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
        var p1 = new TestPipe(false);
        var p2 = new TestPipe(false);
        p1.Other = p2;
        p2.Other = p1;
        var p3 = new TestPipe(true);
        var p4 = new TestPipe(true);
        p3.Other = p4;
        p4.Other = p3;

        var lf = new LocalSyncServerFactory();
        lf.CreateLocalSyncServer(p2,"Test",p3);
        var rf = new RemoteSyncServerFactory();
        rf.CreateRemoteSyncServer(p4,"Test");
        var starter = new SyncMsg(
            SyncMsgType.General,
            SyncProcessStep.Connect,
            JsonSerializer.Serialize(new PipeSeed().TestConfig)
        );
        await p1.SendMsg(starter);
        
    }
}