namespace ServerTest;
using Xunit;
public class UnitTest1
{
    [Theory]
    [InlineData(6)]
    [InlineData(3)]
    [InlineData(5)]
    public void MyFirstTheory(int value)
    {
        Assert.True(true);
        Assert.False(true);
    }
    bool IsOdd(int value)
    {
        return value % 2 == 1;
    }
}