using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Common;

namespace ServerTest
{
    public class TestPipe(bool isAES) : AbsPipeLine(isAES)
    {
        private readonly BlockingCollection<Func<byte[]>> EventQueue =
            new BlockingCollection<Func<byte[]>>();
        private readonly CancellationTokenSource Cts = new CancellationTokenSource();
        public TestPipe? Other;

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
            await Task.Run(() =>
            {
                Cts.Cancel();
            });
        }

        public new async Task ReceiveMsg(SyncMsg msg)
        {
            await Task.Run(() =>
            {
                EventQueue.Add(() =>
                {
                    var r = JsonSerializer.SerializeToUtf8Bytes(msg);
                    if (IsAES)
                    {
                        return AESHelper.EncryptStringToBytes_Aes(r);
                    }
                    else
                    {
                        return r;
                    }
                });
            });
        }

        public override async Task SendMsg(SyncMsg msg)
        {
            if (Other == null)
            {
                throw new Exception("can't be null");
            }
            await Other.ReceiveMsg(msg);
        }

        protected override async Task Listen(Func<byte[], bool> receiveCb)
        {
            while (!Cts.Token.IsCancellationRequested)
            {
                Func<byte[]> eventTask = EventQueue.Take(Cts.Token);
                if (eventTask != null)
                {
                    await Task.Run(() =>
                    {
                        var r = eventTask();
                        receiveCb(r);
                    });
                }
            }
        }
    }
}
