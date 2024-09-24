using System.Text.Json.Serialization;

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

[JsonDerivedType(typeof(AFileOrDir), typeDiscriminator: 1)]
[JsonDerivedType(typeof(File), typeDiscriminator: 2)]
[JsonDerivedType(typeof(Dir), typeDiscriminator: 3)]
public class AFileOrDir
{
    public DirOrFile Type { get; set; }
    public NextOpType? NextOp { get; set; }

    // private string Path = path;
    /// <summary>
    /// 全部为绝对路径... 占用资源会大一点，但是完全OK
    /// </summary>
    ///
    public required string Path { get; set; }

    /// <summary>
    /// 相当于Path 包装，天杀的windows在路径字符串中使用两种分隔符，“/” 和“\”,这导致，即使两个字符串不相同，也可能是同一个路径。现在把它们统一起来
    /// </summary>
    public string FormatedPath
    {
        get { return Path.Replace("\\", "/"); }
        set { Path = value; }
    }

    public bool IsEqual(AFileOrDir other)
    {
        return false;
    }

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
public class File : AFileOrDir
{
    public File()
    {
        Type = DirOrFile.File;
    }

    public required DateTime MTime { get; set; }

    public new bool IsEqual(AFileOrDir other)
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

public class Dir : AFileOrDir
{
    public Dir()
    {
        Type = DirOrFile.Dir;
    }

    public required List<AFileOrDir> Children { get; set; }

    public new bool IsEqual(AFileOrDir other)
    {
        if (other is not Dir otherDir)
        {
            return false;
        }
        else
        {
            if (this.FormatedPath != otherDir.FormatedPath || this.NextOp != otherDir.NextOp)
            {
                return false;
            }
            if (this.Children.Count != otherDir.Children.Count)
            {
                return false;
            }
            this.Children.Sort(AFileOrDir.Compare);
            otherDir.Children.Sort(AFileOrDir.Compare);
            for (int i = 0; i < this.Children.Count; i++)
            {
                if (!this.Children[i].IsEqual(otherDir.Children[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
