namespace Common;

public enum SyncMsgType
{
    Error = 0,
    General = 1,

    //进度消息
    Process = 2,

    //文件展示消息
    DirFileDiff = 3
}

public enum SyncProcessStep
{
    Connect = 1,
    DeployProject = 2,
    DiffFileAndPack = 3,
    PackSqlServer = 4,
    UploadAndUnpack = 5,
    Publish = 6,
    Close = 7
}

public class SyncMsg
{
    public SyncMsgType Type { get; set; }

    public SyncProcessStep Step { get; set; }

    public required string Body { get; set; }
}
