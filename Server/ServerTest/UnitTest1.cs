namespace ServerTest;
using Xunit;
using Common;
using System.Net;

public class UnitTest1
{
    // [Theory]
    // [InlineData(6)]
    // [InlineData(3)]
    // [InlineData(5)]
    // public void MyFirstTheory(int value)
    // {
    //     Assert.True(true);
    //     Assert.False(false);
    // }
    [Fact]
    public void ttest()
    {
        FilesSeed fs = new FilesSeed();
        var (IsSuccess,Message)  =fs.BeforeDir.WriteByThisInfo();
        Assert.True(IsSuccess);
        RootDir nd = new RootDir($"{Directory.GetCurrentDirectory()}/BeforeDir");
        nd.ExtractInfo();
        Assert.True(true);
        Assert.True(nd.IsEqual(fs.BeforeDir));
    }
}