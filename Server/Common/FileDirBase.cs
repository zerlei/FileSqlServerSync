namespace Common;

public enum DirOrFile
{
    Dir = 'd',
    File = 'f',
}

public enum NextOpType
{
    Add = 'a',
    Modify = 'm',
    Del = 'd'
}

public abstract class AFileOrDir(
    string path,
    DirOrFile type = DirOrFile.File,
    NextOpType? nextOp = null
)
{
    public DirOrFile Type { get; set; } = type;
    public NextOpType? NextOp { get; set; } = nextOp;

    /// <summary>
    /// 全部为绝对路径... 占用资源会大一点，但是完全OK
    /// </summary>
    public string Path { get; set; } = path;
    public abstract bool IsEqual(AFileOrDir other);

    public static int Compare(AFileOrDir l, AFileOrDir r)
    {
        var pv = l.Path.CompareTo(r.Path);
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
            return this.MTime == otherFile.MTime
                && this.Path == otherFile.Path
                && this.NextOp == otherFile.NextOp;
        }
    }
}

