using System.Text;
using Common;
using Xunit.Sdk;

namespace ServerTest;

public class FilesSeed
{
    public FilesSeed()
    {
        RootDir.WriteFileStrageFunc = (Common.File file) =>
        {

            //创建或者不创建直接打开文件
            using (FileStream fs = System.IO.File.OpenWrite(file.Path))
            {
                byte[] info = Encoding.UTF8.GetBytes($"this is  {file.Path},Now{DateTime.Now}");
                fs.Write(info, 0, info.Length);
            }

            System.IO.File.SetLastWriteTime(file.Path, file.MTime);
            return (true, "");
        };
        //创建一个文件数
        for (int i = 0; i < 3; ++i)
        {
            var ndir = new Dir($"{Directory.GetCurrentDirectory()}/BeforeDir/{i}");
            if (i == 1)
            {
                var nnfile = new Common.File($"{Directory.GetCurrentDirectory()}/BeforeDir/{i}/{i}.txt", DateTime.Now);
                ndir.Children.Add(nnfile);
            }
            else if (i == 2)
            {
                var nndir = new Dir($"{Directory.GetCurrentDirectory()}/BeforeDir/{i}/{i}");
                var nndir2 = new Dir($"{Directory.GetCurrentDirectory()}/BeforeDir/{i}/{i}_1");
                var nnfile = new Common.File($"{Directory.GetCurrentDirectory()}/BeforeDir/{i}/{i}.txt", DateTime.Now);
                ndir.Children.Add(nnfile);
                var nnfile1 = new Common.File($"{Directory.GetCurrentDirectory()}/BeforeDir/{i}/{i}/{i}.txt", DateTime.Now);
                nndir.Children.Add(nnfile1);
                var nnfile2_1 = new Common.File($"{Directory.GetCurrentDirectory()}/BeforeDir/{i}/{i}_1/{i}_1.txt", DateTime.Now);
                var nnfile2_2 = new Common.File($"{Directory.GetCurrentDirectory()}/BeforeDir/{i}/{i}_1/{i}_2.txt", DateTime.Now);
                nndir2.Children.Add(nnfile2_1);
                nndir2.Children.Add(nnfile2_2);
                ndir.Children.Add(nndir);
                ndir.Children.Add(nndir2);
            }
            BeforeDir.Children.Add(ndir);
        }
    }

    public RootDir BeforeDir = new($"{Directory.GetCurrentDirectory()}/BeforeDir");
    public RootDir AfterDir = new("./AfterDir");
}
