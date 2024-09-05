using Microsoft.AspNetCore.Mvc;
using RemoteServer.Models;
using System.Text;
namespace RemoteServer.Controllers;

public class SyncFilesController(RemoteSyncServerFactory factory, SqliteDbContext db) : ControllerBase
{
    private readonly SqliteDbContext _db = db;
    private readonly RemoteSyncServerFactory Factory = factory;

        [Route("/websoc")]
        public async Task WebsocketConnection(string Name)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                try
                {
                    var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                    Factory.CreateLocalSyncServer(webSocket, Name);
                }
                catch (Exception e)
                {
                    HttpContext.Response.Body = new MemoryStream(Encoding.UTF8.GetBytes(e.Message));
                    HttpContext.Response.StatusCode = StatusCodes.Status406NotAcceptable;
                }
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

    [HttpGet("/GetSyncFilesLogs")]
    public IActionResult GetSyncFilesLogs(
        string? ClientName,
        int? Status,
        DateTime? SyncTimeStart,
        DateTime? SyncTimeEnd,
        int page,
        int rows
    )
    {
        var item =
            from i in _db.SyncLogHeads
            where
                (
                    string.IsNullOrEmpty(ClientName)
                    || (i.ClientName != null && i.ClientName.Contains(ClientName))
                )
                && (Status == null || i.Status == Status)
                && (SyncTimeStart == null || i.SyncTime >= SyncTimeStart)
                && (SyncTimeEnd == null || i.SyncTime <= SyncTimeEnd)
            orderby i.Id descending
            select new
            {
                Head = i,
                Files = (from j in _db.SyncLogFiles where j.HeadId == i.Id select j).ToList()
            };

        return Ok(item.Skip((page - 1) * rows).Take(rows).ToList());
    }

    public class InputFileInfo
    {
        public required string RelativePath { get; set; }
        public DateTime MTime { get; set; }
    }

    public class OutputFileInfo : InputFileInfo
    {
        /// <summary>
        /// 0 新增 1 修改 2 删除
        /// </summary>
        public int ServerOpType { get; set; }
    }

    public class ServerOpFileInfo : OutputFileInfo
    {
        public required string ServerRootDirPath { get; set; }
        public required string ClientRootDirPath { get; set; }
    }

    public class InputFiles
    {
        public required string ServerRootDirPath { get; set; }

        /// <summary>
        /// 0 special 1 exclude
        /// </summary>
        public int Type { get; set; }
        public List<FileInfo>? Files { get; set; }
    }

    public class ServerOpFiles
    {
        public required string ServerRootDirPath { get; set; }
        public string? ClientRootDirPath { get; set; }
        public List<OutputFileInfo>? Files { get; set; }
    }

    [HttpPost("/GetFilesInfoByDir")]
    public IActionResult GetFilesInfoByDir([FromBody] InputFiles inputFiles)
    {
        return Ok(new { IsSuccess = true });
    }

    [HttpPost("/InitASync")]
    public IActionResult InitASync([FromBody] SyncLogHead head)
    {
        try
        {
            var CurrentSyncTaskCount = (
                from i in _db.SyncLogHeads
                where i.Status == 0
                select i
            ).Count();
            if (CurrentSyncTaskCount > 0)
            {
                throw new Exception("存在未完成的任务,请等待完成！");
            }
            head.Id = Guid.NewGuid();
            head.SyncTime = DateTime.Now;
            head.Status = 0;
            _db.SyncLogHeads.Add(head);
            _db.SaveChanges();
            return Ok(new { IsSuccess = true, head.Id });
        }
        catch (Exception e)
        {
            return Ok(new { IsSuccess = false, e.Message });
        }
    }

    [HttpGet("/CloseASync")]
    public IActionResult CloseASync(Guid Id, string Message, int Status)
    {
        try
        {
            var current =
                (from i in _db.SyncLogHeads where i.Id == Id select i).FirstOrDefault()
                ?? throw new Exception("任务不存在！");
            current.Status = Status;
            current.Message = Message;
            _db.SaveChanges();
            return Ok(new { IsSuccess = true });
        }
        catch (Exception e)
        {
            return Ok(new { IsSuccess = false, e.Message });
        }
    }
}
