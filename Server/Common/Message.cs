namespace Common;

public class SyncMsg(bool isSuccess, string body)
{
    public bool IsSuccess { get; set; } = isSuccess;
    public string Body { get; set; } = body;
}
