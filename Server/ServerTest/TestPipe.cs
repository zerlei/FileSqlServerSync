using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Common;

namespace ServerTest
{
    public class TestPipe(bool isAES, string id) : AbsPipeLine(isAES)
    {
        private readonly BlockingCollection<Func<byte[]>> EventQueue =
            new BlockingCollection<Func<byte[]>>();
        private readonly CancellationTokenSource Cts = new CancellationTokenSource();
        public TestPipe? Other;
        public string? ErrResult;
        public string Id = id;

        public override async IAsyncEnumerable<int> Work(
            Func<byte[], bool> receiveCb,
            string addr = ""
        )
        {
            yield return 0;
            await Listen(receiveCb);
            yield return 1;
        }

        public override async Task UploadFile(
            string filePath,
            string dst,
            Func<double, bool> progressCb
        )
        {
            await Task.Run(() =>
            {
                progressCb(100);
                System.IO.File.Copy(filePath, dst, true);
            });
        }

        public override async Task Close(string? CloseReason)
        {
            ErrResult = CloseReason;
            var Id = this.Id;
            Cts.Cancel();
            if (Other != null)
            {
                if (Other.ErrResult == null)
                {
                    await Other.Close(CloseReason);
                }
            }
        }

        public new async Task ReceiveMsg(SyncMsg msg)
        {
            await Task.Run(() =>
            {
                EventQueue.Add(() =>
                {
                    return JsonSerializer.SerializeToUtf8Bytes(msg);
                });
            });
        }

        public override async Task SendMsg(SyncMsg msg)
        {
            if (Other == null)
            {
                throw new Exception("can't be null");
            }
            var r = JsonSerializer.SerializeToUtf8Bytes(msg);

            if (IsAES)
            {
                var str = Encoding.UTF8.GetString(r);
                var t = AESHelper.EncryptStringToBytes_Aes(str);
                var f = AESHelper.DecryptStringFromBytes_Aes(t);
#pragma warning disable CS8604 // 引用类型参数可能为 null。
                await Other.ReceiveMsg(JsonSerializer.Deserialize<SyncMsg>(f));
#pragma warning restore CS8604 // 引用类型参数可能为 null。
            }
            else
            {
#pragma warning disable CS8604 // 引用类型参数可能为 null。
                await Other.ReceiveMsg(JsonSerializer.Deserialize<SyncMsg>(r));
#pragma warning restore CS8604 // 引用类型参数可能为 null。
            }
        }

        protected override async Task Listen(Func<byte[], bool> receiveCb)
        {
            try
            {
                foreach (var eventTask in EventQueue.GetConsumingEnumerable(Cts.Token))
                {
                    await Task.Run(() =>
                    {
                        var r = eventTask();
                        receiveCb(r);
                    });
                }
            }
            catch (OperationCanceledException)
            {
                //var x = 1;
                var id = Id;
                //抛出异常 从 p3 传递到 p2
                throw new Exception(ErrResult);
            }
        }
    }
}
