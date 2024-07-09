namespace ServerTest;

using System.Net;
using Common;
using Xunit;
using Xunit.Sdk;
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
        var (IsSuccess, Message) = filesSeed.BeforeDir.WriteByThisInfo();
        Assert.True(IsSuccess);
        RootDir nd = new(filesSeed.BeforeDir.Path);
        nd.ExtractInfo();
        Assert.True(nd.IsEqual(filesSeed.BeforeDir));
    }

    /// <summary>
    /// 测试文件差异比较
    /// </summary>
    [Fact]
    public void FileDirDiff()
    {
        filesSeed.GetType();
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
