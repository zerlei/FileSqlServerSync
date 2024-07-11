namespace ServerTest;
using Common;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using Xunit;
/// <summary>
/// xUnit将会对每个测试方法创建一个测试上下文，IClassFixture可以用来创建类中共享测试上下文，
/// 
/// XUnit 的测试方法不是按照顺序执行，所以注意对象状态
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
        Dir nd = new(filesSeed.NewDir.FormatedPath);
        nd.ExtractInfo();
        Assert.True(nd.IsEqual(filesSeed.NewDir));
    }

    /// <summary>
    /// 测试文件差异比较
    /// </summary>
     [Fact]
    public void FileDirDiff()
    {
        var cDDir = filesSeed.NewDir.Diff(filesSeed.OldDir);
        
        // Console.WriteLine("################################");
        // Console.WriteLine(cDDir.Children.Count);
        //Assert.True(IsSuccess);

        var str = JsonConvert.SerializeObject(cDDir);
        Assert.True(cDDir.Children.Count !=0);
        Assert.True(filesSeed.DiffDir.IsEqual(cDDir));
        
    }

    /// <summary>
    /// 测试文件合并
    /// </summary>
    [Fact]
    public void DirsCombine()
    {
        var OldDirClone = filesSeed.OldDir.Clone();
        var (IsSuccess, Message) = OldDirClone.Combine(filesSeed.DiffDir);
        Assert.True(IsSuccess);
        //Assert.False(filesSeed.NewDir.IsEqual(filesSeed.OldDir));
        OldDirClone.ResetRootPath("OldDir","NewDir");
        // Console.WriteLine(filesSeed.OldDir.Path);
        Assert.True(OldDirClone.IsEqual(filesSeed.NewDir));
        
    }

    /// <summary>
    /// 测试同步是否成功
    /// </summary>
    [Fact]
    public void FinalSyncFileDir()
    {
        Assert.True(true);
    } 
}
