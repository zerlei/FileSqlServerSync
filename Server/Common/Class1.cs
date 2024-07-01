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
    public required string RelativePath { get; set; }
}

public class File :AFileOrDir
{
    public DateTime MTime { get; set; }
}

public class Dir :AFileOrDir
{
    public List<AFileOrDir> Children { get; set; }
    [SetsRequiredMembers]
    public Dir(string relativePath)
    {
        RelativePath = relativePath;
        Children = [new File { RelativePath = "1.txt", MTime = DateTime.Now,Type = DirOrFile.File }];
        var x = new Dir("");
    }
}
