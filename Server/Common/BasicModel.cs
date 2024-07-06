using System.Diagnostics.CodeAnalysis;

namespace Common;

public enum DirOrFile
{
    Dir,
    File,
}

public abstract class AFileOrDir
{
    public DirOrFile Type { get; set; }
    /// 目录结构的path 根文件夹为绝对路径
    public required string Path { get; set; }
}

public class File : AFileOrDir
{
    public DateTime MTime { get; set; }
}

public class Dir : AFileOrDir
{
    public List<AFileOrDir> Children { get; set; }

    [SetsRequiredMembers]
    public Dir(string path)
    {
        Path = path;
        Children =
        [
            new File
            {
                Path = "1.txt",
                MTime = DateTime.Now,
                Type = DirOrFile.File
            }
        ];
        var x = new Dir("");
        CreatePath = CreatePathDefaultStrategy;
    }
    public bool IsEqual(Dir otherDir)
    {
        return false;
    }
    public bool RestoreFromPath(string path)
    {
        return false;
    }
    public bool CreatePathDefaultStrategy()
    {
        return false;
    }
    public Func<bool> CreatePath;
}
