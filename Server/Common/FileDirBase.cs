namespace Common;

public enum DirOrFile
{
    Dir = 0,
    File = 1,
}

public enum NextOpType
{
    Add = 0,
    Modify = 1,
    Del = 2
}

public abstract class AFileOrDir(
    string path,
    DirOrFile type = DirOrFile.File,
    NextOpType? nextOp = null
)
{
    public DirOrFile Type { get; set; } = type;
    public NextOpType? NextOp { get; set; } = nextOp;

    // private string Path = path;
    /// <summary>
    /// 全部为绝对路径... 占用资源会大一点，但是完全OK
    /// </summary>
    ///
    private string Path = path;

    /// <summary>
    /// 相当于Path 包装，天杀的windows在路径字符串中使用两种分隔符，“/” 和“\”,这导致，即使两个字符串不相同，也可能是同一个路径。现在把它们统一起来
    /// </summary>
    public string FormatedPath
    {
        get { return Path.Replace("\\", "/"); }
        set { Path = value; }
    }
    public abstract bool IsEqual(AFileOrDir other);

    public static int Compare(AFileOrDir l, AFileOrDir r)
    {
        var pv = l.FormatedPath.CompareTo(r.FormatedPath);
        if (pv == 0)
        {
            pv = l.Type.CompareTo(r.Type);
        }
        return pv;
    }
}

/// <summary>
/// 文件
/// </summary>
/// <param name="path">绝对路径</param>
/// <param name="mtime">文件的修改时间</param>/
public class File(string path, DateTime mtime, NextOpType? nextOp = null)
    : AFileOrDir(path, DirOrFile.File, nextOp)
{
    public DateTime MTime { get; set; } = mtime;

    public override bool IsEqual(AFileOrDir other)
    {
        if (other is not File otherFile)
        {
            return false;
        }
        else
        {
            var r =
                this.MTime == otherFile.MTime
                && this.FormatedPath == otherFile.FormatedPath
                && this.NextOp == otherFile.NextOp;

            return r;
        }
    }
}
