namespace Common;

public enum SyncMsgType
{
    Error = 0,
    General = 1,
    Process = 2,
    // DirFilePack = 3
}
public enum SyncProcessStep
{
    Connect = 1,
    DeployProject = 2,
    DiffFileAndPack = 3,
    PackSqlServer = 4,
    UploadAndUnpack = 5,
    Publish = 6
}
public class SyncMsg(SyncMsgType msgType, SyncProcessStep step,  string body)
{
    public SyncMsgType? Type { get; set; } = msgType;

    public SyncProcessStep Step {get;set;} = step;

    public bool IsSuccess
    {
        get { return Type != SyncMsgType.Error; }
    }
    public string Body { get; set; } = body;
}
