using System.Net.WebSockets;
using System.Text;
using Common;
using Microsoft.AspNetCore.Mvc;
using RemoteServer.Models;

namespace RemoteServer.Controllers;

public class SyncFilesController(RemoteSyncServerFactory factory, SqliteDbContext db)
    : ControllerBase
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
                if (Factory.GetServerByName(Name) == null)
                {
                    var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                    var pipeLine = new WebSocPipeLine<WebSocket>(webSocket, true);
                    await Factory.CreateRemoteSyncServer(pipeLine, Name);

                    var x = 11;
                }
                else
                {
                    throw new Exception("RemoteServer: 存在相同名称的发布正在进行!");
                }
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

    [HttpPost("/UploadFile")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                throw new Exception("文件不存在！");
            }
            if (!Directory.Exists(RemoteSyncServer.TempRootFile))
                Directory.CreateDirectory(RemoteSyncServer.TempRootFile);
            var filePath = Path.Combine(RemoteSyncServer.TempRootFile, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            var server = Factory.GetServerById(file.FileName.Split('.').First());
            if (server == null)
            {
                throw new Exception("不存在的Id！");
            }
            else
            {
                var h = new UnPackAndReleaseHelper(server);
                server.SetStateHelpBase(h);
                h.UnPack();
            }

            return Ok(new { IsSuccess = true, Message = "File uploaded successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { IsSuccess = false, Message = $"上传文件失败: {ex.Message}" });
        }
    }
}
