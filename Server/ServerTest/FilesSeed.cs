using System.Text;
using Common;

namespace ServerTest;

public class FilesSeed : IDisposable
{
    public FilesSeed()
    {
        Dir.WriteFileStrageFunc = (Common.File file) =>
          {
              //创建或者不创建直接打开文件
              using (FileStream fs = System.IO.File.OpenWrite(file.Path))
              {
                  byte[] info = Encoding.UTF8.GetBytes($"this is  {file.Path},Now{DateTime.Now}");
                  fs.Write(info, 0, info.Length);
              }
              Console.WriteLine($"WriteFileStrageFunc {file.Path}");
              System.IO.File.SetLastWriteTime(file.Path, file.MTime);
              return (true, "");
          };
        Console.WriteLine("FilesSeed Construct");
        // string TestPath = Path.Combine(Directory.GetCurrentDirectory(), "../../..");
        DateTime beforeTime = DateTime.Now.AddSeconds(-99);
        DateTime afterTime = beforeTime.AddSeconds(11);
        BeforeDir = new Dir(
            TestPath + "/BeforeDir",
            [
                new Dir($"{TestPath}/BeforeDir/0"),
                new Dir(
                    $"{TestPath}/BeforeDir/1",
                    [new Common.File($"{TestPath}/BeforeDir/1/1.txt", beforeTime)]
                ),
                new Dir(
                    $"{TestPath}/BeforeDir/2",
                    [
                        new Common.File($"{TestPath}/BeforeDir/2/2.txt", beforeTime),
                        new Dir(
                            $"{TestPath}/BeforeDir/2/2_1",
                            [
                                new Common.File($"{TestPath}/BeforeDir/2/2_1/1.txt", beforeTime),
                                new Common.File($"{TestPath}/BeforeDir/2/2_1/2.txt", beforeTime),
                            ]
                        ),
                        new Dir(
                            $"{TestPath}/BeforeDir/2/2_2",
                            [
                                new Common.File($"{TestPath}/BeforeDir/2/2_2/1.txt", beforeTime),
                                new Common.File($"{TestPath}/BeforeDir/2/2_2/2.txt", beforeTime),
                                new Dir(
                                    $"{TestPath}/BeforeDir/2/2_2/2_3",
                                    [
                                        new Common.File(
                                            $"{TestPath}/BeforeDir/2/2_2/2_3/1.txt",
                                            beforeTime
                                        ),
                                    ]
                                ),
                            ]
                        )
                    ]
                ),
            ]
        );
        AfterDir = new Dir(
            $"{TestPath}/AfterDir",
            [
                new Dir($"{TestPath}/AfterDir/0"),
                new Dir(
                    $"{TestPath}/AfterDir/1",
                    [
                        //不做修改
                        new Common.File($"{TestPath}/AfterDir/1/1.txt", beforeTime),
                        //将要删除
                        new Common.File($"{TestPath}/AfterDir/1/2_D.txt", beforeTime),
                    ]
                ),
                new Dir(
                    $"{TestPath}/AfterDir/2",
                    [   
                        // 将要添加
                        // new Common.File($"{TestPath}/AfterDir/2/2.txt", beforeTime),
                        new Dir(
                            $"{TestPath}/AfterDir/2/2_1",
                            [
                                new Common.File($"{TestPath}/AfterDir/2/2_1/1.txt", beforeTime),
                                new Common.File($"{TestPath}/AfterDir/2/2_1/2.txt", afterTime),
                            ]
                        ),
                        new Dir(
                            $"{TestPath}/AfterDir/2/2_2",
                            [
                                new Common.File($"{TestPath}/AfterDir/2/2_2/1.txt", afterTime),
                                new Common.File($"{TestPath}/AfterDir/2/2_2/2.txt", afterTime),
                                new Dir(
                                    $"{TestPath}/AfterDir/2/2_2/2_3",
                                    [
                                        new Common.File(
                                            $"{TestPath}/AfterDir/2/2_2/2_3/1.txt",
                                           afterTime
                                        ),
                                    ]
                                ),
                            ]
                        )
                    ]
                ),
            ]
        );
    }

    private readonly string TestPath = Path.Combine(Directory.GetCurrentDirectory(), "../../..");
    public Dir BeforeDir;
    public Dir AfterDir;

    public void Dispose()
    {
        Directory.Delete(BeforeDir.Path, true);
        Console.WriteLine("FilesSeed Dispose");
        GC.SuppressFinalize(this);
    }
    // ~FilesSeed()
    // {
    //      Console.WriteLine("FilesSeed ~");
    // }
}
