using Common;
using LocalServer;
using RemoteServer;

namespace ServerTest;

public class PipeSeed : IDisposable
{
    public PipeSeed()
    {
        TestConfig = new Config
        {
            Name = "Test",
            RemoteUrl = "D:/FileSyncTest/dtemp",
            RemotePwd = "t123",
            IsDeployDb = true,
            IsDeployProject = true,
            LocalProjectAbsolutePath = "D:/git/HMES-H7-HNFY/HMES-H7-HNFYMF/HMES-H7-HNFYMF.WEB",
            LocalRootPath = "D:/FileSyncTest/src",

            RemoteRootPath = "D:/FileSyncTest/dst",
            SrcDb = new MSSqlConfig
            {
                ServerName = "172.16.12.2",
                DatabaseName = "HMES_H7_HNFYMF",
                User = "hmes-h7",
                Password = "Hmes-h7666",
                TrustServerCertificate = "True",
                SyncTablesData = new List<string>
                {
                    "dbo.sys_Button",
                    "dbo.sys_Menu",
                    "dbo.sys_Module",
                    "dbo.sys_Page",
                }
            },
            DstDb = new MSSqlConfig
            {
                ServerName = "127.0.0.1",
                DatabaseName = "HMES_H7_HNFYMF",
                User = "sa",
                Password = "0",
                TrustServerCertificate = "True"
            },
            DirFileConfigs = new List<DirFileConfig>
            {
                new DirFileConfig { DirPath = "/bin", Excludes = ["/roslyn", "/Views"] }
            },

            // C:/Windows/System32/inetsrv/appcmd.exe stop sites "publicserver"
            // C:/Windows/System32/inetsrv/appcmd.exe start sites "publicserver"
            ExecProcesses = new List<ExecProcess>
            {
                new ExecProcess
                {
                    Argumnets = "ls",
                    FileName = "powershell",
                    StepBeforeOrAfter = "A",
                    ExecInLocalOrServer = "L",
                    Step = SyncProcessStep.DeployProject,
                },
                new ExecProcess
                {
                    Argumnets = "ls",
                    FileName = "powershell",
                    StepBeforeOrAfter = "B",
                    ExecInLocalOrServer = "S",
                    Step = SyncProcessStep.Publish,
                },
                new ExecProcess
                {
                    Argumnets = "ls",
                    FileName = "powershell",
                    StepBeforeOrAfter = "A",
                    ExecInLocalOrServer = "S",
                    Step = SyncProcessStep.Publish,
                },
            }
        };
    }

    public Config TestConfig;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
