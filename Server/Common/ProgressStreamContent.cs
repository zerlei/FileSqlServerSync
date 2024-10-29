using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

namespace Common;

public class ProgressStreamContent(Stream stream_, IProgress<double> progress)
    : StreamContent(stream_, 5 * 1024 * 1024)
{
    private readonly Stream FileStream = stream_;
    private readonly int BufferSize = 5 * 1024 * 1024;
    private readonly IProgress<double> Progress = progress;

    protected override async Task SerializeToStreamAsync(
        Stream stream,
        TransportContext? context = null
    )
    {
        var buffer = new byte[BufferSize];
        long totalBytesRead = 0;
        long totalBytes = FileStream.Length;
        int bytesRead;

        while ((bytesRead = await FileStream.ReadAsync(buffer.AsMemory())) != 0)
        {
            await stream.WriteAsync(buffer.AsMemory(0, bytesRead));
            totalBytesRead += bytesRead;
            Progress.Report((double)totalBytesRead / totalBytes);
        }
    }
}
