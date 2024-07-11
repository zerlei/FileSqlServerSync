namespace ServerTest;
using Common;
using Xunit;
/// <summary>
/// xUnit将会对每个测试方法创建一个测试上下文，IClassFixture可以用来创建类中共享测试上下文，
/// </summary>
public class DirFileOpTest(FilesSeed filesSeed) : IClassFixture<FilesSeed>
{
    private readonly FilesSeed filesSeed = filesSeed;


    /// <summary>
    /// 测试文件目录写入和提取
    /// </summary>
    [Fact]
    public void FileDirWriteExtract()
    {
        var (IsSuccess, Message) = filesSeed.NewDir.WriteByThisInfo();
        Assert.True(IsSuccess);
        Dir nd = new(filesSeed.NewDir.Path);
        nd.ExtractInfo();
        Assert.True(nd.IsEqual(filesSeed.NewDir));
    }

    /// <summary>
    /// 测试文件差异比较
    /// </summary>
    [Fact]
    public void FileDirDiff()
    {
        var (IsSuccess,cDDir) = filesSeed.NewDir.Diff(filesSeed.OldDir);
        Assert.True(IsSuccess);
        Assert.True(cDDir is not null);
        filesSeed.DiffDir.IsEqual(cDDir);
        
    }

    /// <summary>
    /// 测试文件合并
    /// </summary>
    [Fact]
    public void DirsCombine()
    {
        var (IsSuccess, Message) = filesSeed.OldDir.Combine(filesSeed.DiffDir);
        Assert.True(IsSuccess);

        Assert.False(filesSeed.NewDir.IsEqual(filesSeed.OldDir));
        filesSeed.OldDir.ResetRootPath("OldDir","NewDir");
        Console.WriteLine(filesSeed.OldDir.Path);
        Assert.True(filesSeed.OldDir.IsEqual(filesSeed.NewDir));
        
    }

    /// <summary>
    /// 测试同步是否成功
    /// </summary>
    [Fact]
    public void FinalSyncFileDir()
    {
        filesSeed.GetType();
    }
}
